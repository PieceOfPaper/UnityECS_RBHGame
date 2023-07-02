using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ECSAfterProcessSystemGroup))]
public partial struct ECSBulletSystem : ISystem
{
    [BurstCompile]
    private partial struct BulletUpdateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;
        
        private void Execute([ChunkIndexInQuery] int sortKey, in Entity refEntity, in LocalTransform refTransform, ref ECSBulletData refBulletData)
        {
            var bulletData = refBulletData;
            
            bool isDestroy = false;
            if (isDestroy == false && bulletData.hitCount != 0)
            {
                if (bulletData.currentHitCount >= bulletData.hitCount)
                    isDestroy = true;
            }
            if (isDestroy == false && bulletData.duration > 0f)
            {
                bulletData.currentTime += deltaTime;
                if (bulletData.currentTime >= bulletData.duration)
                    isDestroy = true;
            }
            
            if (isDestroy == true)
            {
                ecb.DestroyEntity(sortKey, refEntity);
            }
            else
            {
                refBulletData = bulletData;
            }
        }
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        new BulletUpdateJob()
        {
            ecb = ecb,
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();
    }
}
