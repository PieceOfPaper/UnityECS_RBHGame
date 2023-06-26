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

    public struct MovableEntity
    {
        public Entity entity;
        public float radius;
        public float3 position;
        public quaternion rotation;
        public float moveSpeed;
        public int attackableLayer;
        public int attackDamage;
    }
    
    [BurstCompile]
    private partial struct CollisionMovableEntityJob : IJobEntity
    {
        public float deltaTime;
        [ReadOnly] public NativeArray<MovableEntity> movableEntityDataArray;
        public NativeArray<ECSFloatingDamageData> floatingDamageArray;
        
        private void Execute([EntityIndexInQuery]int index, in Entity refEntity, ref ECSCharacterData refCharacterData, in LocalTransform refTransform, ref ECSMoveData refMoveData)
        {
            var myMoveData = refMoveData;
            var myCharacterData = refCharacterData;
            var myLayer = (int)myCharacterData.layer;
            var myEntityRadius = refCharacterData.radius;
            
            if (myCharacterData.damagedTimer > 0f)
                myCharacterData.damagedTimer = math.max(0f, myCharacterData.damagedTimer - deltaTime);
            
            for (int i = 0; i < movableEntityDataArray.Length; i ++)
            {
                var movableEntity = movableEntityDataArray[i];
                if (movableEntity.entity == refEntity) continue;
            
                if (math.distancesq(movableEntity.position, refTransform.Position) < (myEntityRadius + movableEntity.radius) * (myEntityRadius + movableEntity.radius))
                {
                    myMoveData.force += math.normalize(refTransform.Position - movableEntity.position) * movableEntity.moveSpeed * 0.1f;
                    if (myCharacterData.hp > 0 && myCharacterData.damagedTimer <= 0f && (movableEntity.attackableLayer & (1 << myLayer)) > 0)
                    {
                        myCharacterData.hp = math.max(0, myCharacterData.hp - movableEntity.attackDamage);
                        myCharacterData.damagedTimer += 1.0f; //TODO - 어딘가에 정의해두자.
                        if (myCharacterData.hp == 0)
                            myCharacterData.isDead = true;
                        
                        floatingDamageArray[index] = new ECSFloatingDamageData()
                        {
                            layer = myCharacterData.layer,
                            position = refTransform.Position,
                            damage = movableEntity.attackDamage,
                        };
                    }
                }
            }
            
            refMoveData = myMoveData;
            refCharacterData = myCharacterData;
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
        var characterDataArray = characterQuery.ToComponentDataArray<ECSCharacterData>(Allocator.Temp);
        var characterTransformArray = characterQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        var characterMoveDataArray = characterQuery.ToComponentDataArray<ECSMoveData>(Allocator.Temp);
        var movableEntityDataArray = new NativeArray<MovableEntity>(characterEntityArray.Length, Allocator.TempJob);
        for (int i = 0; i < characterEntityArray.Length; i ++)
        {
            movableEntityDataArray[i] = new MovableEntity()
            {
                entity = characterEntityArray[i],
                radius = characterDataArray[i].radius,
                position = characterTransformArray[i].Position,
                rotation = characterTransformArray[i].Rotation,
                moveSpeed = characterMoveDataArray[i].currentSpeed,
                attackableLayer = characterDataArray[i].attackableLayer,
                attackDamage = characterDataArray[i].attackDamage,
            };
        }
        characterEntityArray.Dispose();
        characterDataArray.Dispose();
        characterTransformArray.Dispose();
        characterMoveDataArray.Dispose();

        var floatingDamageArray = new NativeArray<ECSFloatingDamageData>(characterEntityArray.Length, Allocator.TempJob);
        var collisionMovableEntityJob = new CollisionMovableEntityJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            movableEntityDataArray = movableEntityDataArray,
            floatingDamageArray = floatingDamageArray,
        };
        state.Dependency = collisionMovableEntityJob.ScheduleParallel(characterQuery, state.Dependency);
        state.Dependency = movableEntityDataArray.Dispose(state.Dependency);
        characterQuery.Dispose();
        
        var floatingDamageCopyJob = new FloatingDamageCopyJob()
        {
            floatingDamageArray = floatingDamageArray,
            floatingDamageList = FloatingDamageList,
        };
        state.Dependency = floatingDamageCopyJob.Schedule(state.Dependency);
        state.Dependency = floatingDamageArray.Dispose(state.Dependency);
    }
}
