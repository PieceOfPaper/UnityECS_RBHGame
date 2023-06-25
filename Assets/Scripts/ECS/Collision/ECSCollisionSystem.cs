using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ECSPreProcessSystemGroup))]
public partial struct ECSCollisionSystem : ISystem
{
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
        
        private void Execute(in Entity refEntity, ref ECSCharacterData refCharacterData, in LocalTransform refTransform, ref ECSMoveData refMoveData)
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
                        myCharacterData.hp -= movableEntity.attackDamage;
                        myCharacterData.damagedTimer += 1.0f; //TODO - 어딘가에 정의해두자.
                        if (myCharacterData.hp == 0)
                            myCharacterData.isDead = true;
                    }
                }
            }
            
            refMoveData = myMoveData;
            refCharacterData = myCharacterData;
        }
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

        var collisionMovableEntityJob = new CollisionMovableEntityJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            movableEntityDataArray = movableEntityDataArray,
        };
        state.Dependency = collisionMovableEntityJob.ScheduleParallel(characterQuery, state.Dependency);
        movableEntityDataArray.Dispose(state.Dependency);
        characterQuery.Dispose();
    }
}
