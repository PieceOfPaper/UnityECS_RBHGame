using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ECSAfterProcessSystemGroup))]
public partial struct ECSCharacterSystem : ISystem
{
    [BurstCompile]
    private partial struct CaracterUpdateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;
        
        private void Execute([ChunkIndexInQuery] int sortKey, in Entity refEntity, ref ECSCharacterData refCharacterData, in LocalTransform refTransform)
        {
            if (refCharacterData.damagedTimer > 0f)
            {
                refCharacterData.damagedTimer = math.max(0f, refCharacterData.damagedTimer - deltaTime);
            }
            
            if (refCharacterData.isDead == true)
            {
                if (refCharacterData.deadEffect != Entity.Null)
                {
                    var deadEffectEntity = ecb.Instantiate(sortKey, refCharacterData.deadEffect);
                    var deadEffectTransform = new LocalTransform()
                    {
                        Position = refTransform.Position,
                        Rotation = refTransform.Rotation,
                        Scale = refTransform.Scale,
                    };
                    ecb.SetComponent(sortKey, deadEffectEntity, deadEffectTransform);
                }
                if (refCharacterData.dropItem != Entity.Null)
                {
                    var dropItemEntity = ecb.Instantiate(sortKey, refCharacterData.dropItem);
                    var dropItemTransform = new LocalTransform()
                    {
                        Position = refTransform.Position,
                        Rotation = refTransform.Rotation,
                        Scale = refTransform.Scale,
                    };
                    ecb.SetComponent(sortKey, dropItemEntity, dropItemTransform);
                }
                ecb.DestroyEntity(sortKey, refEntity);
            }
        }
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        new CaracterUpdateJob()
        {
            ecb = ecb,
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();
    }
}
