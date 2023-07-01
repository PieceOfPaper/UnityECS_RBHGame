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
        
        private void Execute([ChunkIndexInQuery] int sortKey, in Entity refEntity, in ECSCharacterData refCharacterData)
        {
            if (refCharacterData.isDead == true)
            {
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
