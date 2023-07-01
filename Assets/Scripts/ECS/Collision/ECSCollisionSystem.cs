using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

public struct ECSFloatingDamageData
{
    public ECSCharacterLayer layer;
    public float3 position;
    public int damage;
}

[UpdateInGroup(typeof(ECSPreProcessSystemGroup))]
public partial struct ECSCollisionSystem : ISystem
{
    public static NativeList<ECSFloatingDamageData> FloatingDamageList;

    private struct CollisionData
    {
        public byte type; //1-character, 2-bullet
        public Entity entity;
        public float radius;
        public float3 position;
        public quaternion rotation;
        public float moveSpeed;
        public int attackableLayer;
        public int attackDamage;
        
        public ECSCharacterData characterData;
        public ECSBulletData bulletData;
    }

    private struct CollisionResultData
    {
        public bool isCollision;
        public int sortKey;
        public Entity entity;
        public ECSCharacterData characterData;
        public LocalTransform localTransform;
        public CollisionData collisionData;
    }

    [BurstCompile]
    private partial struct CollisionJob : IJobEntity
    {
        public float deltaTime;
        [ReadOnly] public NativeArray<CollisionData> collisionDataArray;
        public NativeArray<CollisionResultData> collisionResultDataArray;

        private void Execute([EntityIndexInQuery]int index, [ChunkIndexInQuery] int sortKey, in Entity refEntity, ref ECSCharacterData refCharacterData, in LocalTransform refTransform, ref ECSMoveData refMoveData)
        {
            var moveData = refMoveData;
            var myEntityRadius = refCharacterData.radius;

            collisionResultDataArray[index] = new CollisionResultData()
            {
                isCollision = false,
                sortKey = sortKey,
                entity = refEntity,
                characterData = refCharacterData,
            };
            
            bool isCollision = false;
            for (int i = 0; i < collisionDataArray.Length; i ++)
            {
                var collisionData = collisionDataArray[i];
                if (collisionData.entity == refEntity) continue;
                if (collisionData.type == 2 && collisionData.bulletData.IsWillDestroy() == true) continue;
            
                if (math.distancesq(collisionData.position, refTransform.Position) < (myEntityRadius + collisionData.radius) * (myEntityRadius + collisionData.radius))
                {
                    //캐릭터간 충돌처리
                    if (collisionData.type == 1)
                    {
                        var dir = math.normalize(new float3(refTransform.Position.x, 0f, refTransform.Position.z) - new float3(collisionData.position.x, 0f, collisionData.position.z));
                        moveData.force += dir * collisionData.moveSpeed * 0.1f;
                    }

                    if (isCollision == false)
                    {
                        isCollision = true;
                        collisionResultDataArray[index] = new CollisionResultData()
                        {
                            isCollision = true,
                            sortKey = sortKey,
                            entity = refEntity,
                            characterData = refCharacterData,
                            localTransform = refTransform,
                            collisionData = collisionData,
                        };
                    }
                }
            }

            refMoveData = moveData;
        }
    }
    
    [BurstCompile]
    private partial struct CollisionResultJob : IJob
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;
        public NativeArray<CollisionResultData> collisionResultDataArray;
        public NativeArray<ECSFloatingDamageData> floatingDamageArray;

        public void Execute()
        {
            for (int i = 0; i < collisionResultDataArray.Length; i ++)
            {
                var collisionResultData = collisionResultDataArray[i];
                var characterData = collisionResultData.characterData;
                var collisionData = collisionResultData.collisionData;

                if (collisionResultData.isCollision == false) continue;

                var characterLayer = (int)characterData.layer;
                if (characterData.hp > 0 && characterData.damagedTimer <= 0f && (collisionData.attackableLayer & (1 << characterLayer)) > 0)
                {
                    characterData.hp = math.max(0, characterData.hp - collisionData.attackDamage);
                    characterData.damagedTimer = characterData.damagedCooltime;
                    if (characterData.hp == 0)
                        characterData.isDead = true;
                    ecb.SetComponent(collisionResultData.sortKey, collisionResultData.entity, characterData);
                    
                    floatingDamageArray[i] = new ECSFloatingDamageData()
                    {
                        layer = characterData.layer,
                        position = collisionResultData.localTransform.Position,
                        damage = collisionData.attackDamage,
                    };

                    if (collisionData.type == 2)
                    {
                        var collisionBulletData = collisionData.bulletData;
                        collisionBulletData.currentHitCount += 1;
                        ecb.SetComponent(0, collisionData.entity, collisionBulletData);
                    }
                }
            }
        }
    }
    
    [BurstCompile]
    private partial struct FloatingDamageCopyJob : IJob
    {
        public NativeArray<ECSFloatingDamageData> floatingDamageArray;
        public NativeList<ECSFloatingDamageData> floatingDamageList;
        
        public void Execute()
        {
            floatingDamageList.Clear();
            for (int i = 0; i < floatingDamageArray.Length; i ++)
            {
                var floatingData = floatingDamageArray[i];
                if (floatingData.layer == 0 && floatingData.damage == 0) continue;
                floatingDamageList.Add(floatingData);
            }
        }
    }

    public void OnCreate(ref SystemState state)
    {
        FloatingDamageList = new NativeList<ECSFloatingDamageData>(Allocator.Persistent);
    }

    public void OnDestroy(ref SystemState state)
    {
        FloatingDamageList.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {   
        var characterQuery = state.EntityManager.CreateEntityQuery(typeof(ECSCharacterData), typeof(ECSMoveData), typeof(LocalTransform));
        var characterEntityArray = characterQuery.ToEntityArray(Allocator.Temp);
        var bulletQuery = state.EntityManager.CreateEntityQuery(typeof(ECSBulletData), typeof(ECSMoveData), typeof(LocalTransform));
        var bulletEntityArray = bulletQuery.ToEntityArray(Allocator.Temp);

        int characterCount = characterEntityArray.Length;
        int bulletCount = bulletEntityArray.Length;
        int queryCount = characterCount + bulletCount;
        var collisionDataArray = new NativeArray<CollisionData>(queryCount, Allocator.TempJob);
        
        var characterDataArray = characterQuery.ToComponentDataArray<ECSCharacterData>(Allocator.Temp);
        var characterTransformArray = characterQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        var characterMoveDataArray = characterQuery.ToComponentDataArray<ECSMoveData>(Allocator.Temp);
        for (int i = 0; i < characterEntityArray.Length; i ++)
        {
            collisionDataArray[i] = new CollisionData()
            {
                type = 1,
                entity = characterEntityArray[i],
                radius = characterDataArray[i].radius,
                position = characterTransformArray[i].Position,
                rotation = characterTransformArray[i].Rotation,
                moveSpeed = characterMoveDataArray[i].currentSpeed,
                attackableLayer = characterDataArray[i].attackableLayer,
                attackDamage = characterDataArray[i].attackDamage,
                characterData = characterDataArray[i],
            };
        }
        characterEntityArray.Dispose();
        characterDataArray.Dispose();
        characterTransformArray.Dispose();
        characterMoveDataArray.Dispose();
        
        var bulletDataArray = bulletQuery.ToComponentDataArray<ECSBulletData>(Allocator.Temp);
        var bulletTransformArray = bulletQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        var bulletMoveDataArray = bulletQuery.ToComponentDataArray<ECSMoveData>(Allocator.Temp);
        for (int i = 0; i < bulletEntityArray.Length; i ++)
        {
            collisionDataArray[characterEntityArray.Length + i] = new CollisionData()
            {
                type = 2,
                entity = bulletEntityArray[i],
                radius = bulletDataArray[i].radius,
                position = bulletTransformArray[i].Position,
                rotation = bulletTransformArray[i].Rotation,
                moveSpeed = bulletMoveDataArray[i].currentSpeed,
                attackableLayer = bulletDataArray[i].attackableLayer,
                attackDamage = bulletDataArray[i].attackDamage,
                bulletData = bulletDataArray[i],
            };
        }
        bulletEntityArray.Dispose();
        bulletDataArray.Dispose();
        bulletTransformArray.Dispose();
        bulletMoveDataArray.Dispose();
        
        var collisionResultDataArray = new NativeArray<CollisionResultData>(characterCount, Allocator.TempJob);
        var collisionJob = new CollisionJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            collisionDataArray = collisionDataArray,
            collisionResultDataArray = collisionResultDataArray,
        };
        state.Dependency = collisionJob.ScheduleParallel(characterQuery, state.Dependency);
        state.Dependency = collisionDataArray.Dispose(state.Dependency);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var floatingDamageArray = new NativeArray<ECSFloatingDamageData>(characterCount, Allocator.TempJob);
        var collisionResultJob = new CollisionResultJob()
        {
            ecb = ecb,
            collisionResultDataArray = collisionResultDataArray,
            floatingDamageArray = floatingDamageArray,
        };
        state.Dependency = collisionResultJob.Schedule(state.Dependency);
        state.Dependency = collisionResultDataArray.Dispose(state.Dependency);
        
        var floatingDamageCopyJob = new FloatingDamageCopyJob()
        {
            floatingDamageArray = floatingDamageArray,
            floatingDamageList = FloatingDamageList,
        };
        state.Dependency = floatingDamageCopyJob.Schedule(state.Dependency);
        state.Dependency = floatingDamageArray.Dispose(state.Dependency);
        
        characterQuery.Dispose();
        bulletQuery.Dispose();
    }
}
