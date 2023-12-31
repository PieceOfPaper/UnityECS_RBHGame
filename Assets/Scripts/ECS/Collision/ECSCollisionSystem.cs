using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(ECSProcessSystemGroup))]
[UpdateAfter(typeof(ECSMoveSystem))]
public partial struct ECSCollisionSystem : ISystem
{
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
    private partial struct CollisionWallJob : IJobEntity
    {
        public float deltaTime;
        [ReadOnly] public NativeArray<ECSWallData> wallDataArray;
        [ReadOnly] public NativeArray<LocalTransform> wallTransformArray;
        
        private void Execute(in ECSCharacterData refCharacterData, ref LocalTransform refTransform, in ECSMoveData refMoveData)
        {
            var transform = refTransform;
            var radius = refCharacterData.radius;
            var moveDir = refMoveData.useCustomdir ? refMoveData.customDir : math.mul(refTransform.Rotation, new float3(0f, 0f, 1f));

            for (int i = 0; i < wallDataArray.Length; i ++)
            {
                var wallData = wallDataArray[i];
                var wallTransform = wallTransformArray[i];

                var position = transform.Position;
                position.y = 0f;
                
                var wallMatrix = float4x4.TRS(wallTransform.Position, wallTransform.Rotation, wallTransform.Scale);
                var relativePos = wallMatrix.InverseTransformPoint(position);
                relativePos.y = 0f;

                switch (wallData.rangeType)
                {
                    case ECSRangeType.Circle:
                        {
                            var distanceSqr = math.distancesq(relativePos, float3.zero);
                            if (distanceSqr < ((wallData.rangeArg1 + radius) * (wallData.rangeArg1 + radius)))
                            {
                                transform.Position = wallTransform.Position + math.normalize(position - new float3(wallTransform.Position.x, 0f, wallTransform.Position.z)) * (wallData.rangeArg1 + radius);
                            }
                        }
                        break;
                    case ECSRangeType.Box:
                        if (math.abs(relativePos.x) < (wallData.rangeArg1 * 0.5f + radius) && math.abs(relativePos.z) < (wallData.rangeArg2 * 0.5f + radius))
                        {
                            var corner1 = new float3(-wallData.rangeArg1 * 0.5f, 0f, -wallData.rangeArg2 * 0.5f);
                            var corner2 = new float3(wallData.rangeArg1 * 0.5f, 0f, -wallData.rangeArg2 * 0.5f);
                            var corner3 = new float3(-wallData.rangeArg1 * 0.5f, 0f, wallData.rangeArg2 * 0.5f);
                            var corner4 = new float3(wallData.rangeArg1 * 0.5f, 0f, wallData.rangeArg2 * 0.5f);

                            // 모서리는 원 처럼 처리하자.
                            if (relativePos.x < corner1.x && relativePos.z < corner1.z)
                            {
                                var distanceSqr = math.distancesq(relativePos, corner1);
                                if (distanceSqr < radius * radius)
                                    relativePos = corner1 + math.normalize(relativePos - corner1) * radius;
                            }
                            else if (relativePos.x > corner2.x && relativePos.z < corner2.z)
                            {
                                var distanceSqr = math.distancesq(relativePos, corner2);
                                if (distanceSqr < radius * radius)
                                    relativePos = corner2 + math.normalize(relativePos - corner2) * radius;
                            }
                            else if (relativePos.x < corner3.x && relativePos.z > corner3.z)
                            {
                                var distanceSqr = math.distancesq(relativePos, corner3);
                                if (distanceSqr < radius * radius)
                                    relativePos = corner3 + math.normalize(relativePos - corner3) * radius;
                            }
                            else if (relativePos.x > corner4.x && relativePos.z > corner4.z)
                            {
                                var distanceSqr = math.distancesq(relativePos, corner4);
                                if (distanceSqr < radius * radius)
                                    relativePos = corner4 + math.normalize(relativePos - corner4) * radius;
                            }
                            else
                            {
                                if (relativePos.x < 0 && (relativePos.x + radius) > corner1.x && 
                                    relativePos.z >= corner1.z && relativePos.z <= corner4.z)
                                {
                                    relativePos.x = corner1.x - radius;
                                }
                                else if (relativePos.x > 0 && (relativePos.x - radius) < corner4.x && 
                                         relativePos.z >= corner1.z && relativePos.z <= corner4.z)
                                {
                                    relativePos.x = corner4.x + radius;
                                }
                                else if (relativePos.z < 0 && (relativePos.z + radius) > corner1.z && 
                                         relativePos.x >= corner1.x && relativePos.x <= corner4.x)
                                {
                                    relativePos.z = corner1.z - radius;
                                }
                                else if (relativePos.z > 0 && (relativePos.z - radius) < corner4.z && 
                                         relativePos.x >= corner1.x && relativePos.x <= corner4.x)
                                {
                                    relativePos.z = corner4.z + radius;
                                }
                            }
                            transform.Position = wallMatrix.TransformPoint(relativePos);
                        }
                        break;
                }
            }
            
            refTransform = transform;
        }
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
            
                if (math.distancesq(new float2(collisionData.position.x, collisionData.position.z), new float2(refTransform.Position.x, refTransform.Position.z)) < (myEntityRadius + collisionData.radius) * (myEntityRadius + collisionData.radius))
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
                    
                    var newFloatingDamageEntity = ecb.CreateEntity(collisionResultData.sortKey);
                    ecb.AddComponent<ECSFloatingDamageData>(collisionResultData.sortKey, newFloatingDamageEntity);
                    ecb.SetComponent(collisionResultData.sortKey, newFloatingDamageEntity, new ECSFloatingDamageData()
                    {
                        layer = characterData.layer,
                        position = collisionResultData.localTransform.Position,
                        damage = collisionData.attackDamage,
                    });
                    
                    var hitPoint = math.lerp(collisionData.position, collisionResultData.localTransform.Position,collisionData.radius / (collisionData.radius + collisionResultData.characterData.radius));
                    
                    if (collisionData.type == 1)
                    {
                        if (collisionData.characterData.hitEffect != Entity.Null)
                        {
                            var hitEffectEntity = ecb.Instantiate(0, collisionData.characterData.hitEffect);
                            var hitEffectTransform = new LocalTransform()
                            {
                                Position = hitPoint,
                                Rotation = Quaternion.identity,
                                Scale = 1.0f,
                            };
                            ecb.SetComponent(0, hitEffectEntity, hitEffectTransform);
                        }
                    }
                    else if (collisionData.type == 2)
                    {
                        var collisionBulletData = collisionData.bulletData;
                        if (collisionBulletData.hitEffect != Entity.Null)
                        {
                            var hitEffectEntity = ecb.Instantiate(0, collisionBulletData.hitEffect);
                            var hitEffectTransform = new LocalTransform()
                            {
                                Position = hitPoint,
                                Rotation = Quaternion.identity,
                                Scale = 1.0f,
                            };
                            ecb.SetComponent(0, hitEffectEntity, hitEffectTransform);
                        }
                        collisionBulletData.currentHitCount += 1;
                        ecb.SetComponent(0, collisionData.entity, collisionBulletData);
                    }
                }
            }
        }
    }

    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        
        var characterQuery = state.EntityManager.CreateEntityQuery(typeof(ECSCharacterData), typeof(ECSMoveData), typeof(LocalTransform));
        var characterEntityArray = characterQuery.ToEntityArray(Allocator.Temp);
        var bulletQuery = state.EntityManager.CreateEntityQuery(typeof(ECSBulletData), typeof(ECSMoveData), typeof(LocalTransform));
        var bulletEntityArray = bulletQuery.ToEntityArray(Allocator.Temp);

        var wallQuery = state.EntityManager.CreateEntityQuery(typeof(ECSWallData), typeof(LocalTransform));
        var wallDataArray = wallQuery.ToComponentDataArray<ECSWallData>(Allocator.TempJob);
        var wallTransformArray = wallQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

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


        var collisionWallJob = new CollisionWallJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            wallDataArray = wallDataArray,
            wallTransformArray = wallTransformArray,
        };
        state.Dependency = collisionWallJob.ScheduleParallel(characterQuery, state.Dependency);
        state.Dependency = wallDataArray.Dispose(state.Dependency);
        state.Dependency = wallTransformArray.Dispose(state.Dependency);
        
        var collisionResultDataArray = new NativeArray<CollisionResultData>(characterCount, Allocator.TempJob);
        var collisionJob = new CollisionJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            collisionDataArray = collisionDataArray,
            collisionResultDataArray = collisionResultDataArray,
        };
        state.Dependency = collisionJob.ScheduleParallel(characterQuery, state.Dependency);
        state.Dependency = collisionDataArray.Dispose(state.Dependency);

        var collisionResultJob = new CollisionResultJob()
        {
            ecb = ecb,
            collisionResultDataArray = collisionResultDataArray,
        };
        state.Dependency = collisionResultJob.Schedule(state.Dependency);
        state.Dependency = collisionResultDataArray.Dispose(state.Dependency);
        
        characterQuery.Dispose();
        bulletQuery.Dispose();
        wallQuery.Dispose();
    }
}
