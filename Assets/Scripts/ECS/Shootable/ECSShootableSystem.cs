using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ECSSpawnSystemGroup))]
[UpdateAfter(typeof(ECSSpawnSystem))]
public partial struct ECSShootableSystem : ISystem
{
    private partial struct ShootableJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;

        private void Execute([ChunkIndexInQuery] int sortKey, in LocalTransform refLocalTransform, ref ECSShootableData refShootableData)
        {
            var shootableData = refShootableData;
            
            if (shootableData.currentShootCooltime > 0f) shootableData.currentShootCooltime -= deltaTime;
            if (shootableData.currentReloadTime > 0f)
            {
                shootableData.currentReloadTime -= deltaTime;
                if (shootableData.currentReloadTime <= 0f)
                    shootableData.remainShootCount = shootableData.reloadCount;
            }
            
            if (shootableData.pressShoot == true && shootableData.remainShootCount > 0 && shootableData.currentShootCooltime <= 0f && shootableData.currentReloadTime <= 0f)
            {
                float startAngle = -shootableData.shootSpreadRange * (shootableData.shootCount - 1) * 0.5f;
                for (int i = 0; i < shootableData.shootCount; i ++)
                {
                    var bulletEntity = ecb.Instantiate(sortKey, shootableData.bullet);
                    var bulletTransform = refLocalTransform;
                    bulletTransform = bulletTransform.RotateY((startAngle + i * shootableData.shootSpreadRange) * Mathf.Deg2Rad);
                    bulletTransform = bulletTransform.Translate(math.mul(bulletTransform.Rotation, shootableData.shootPoint));
                    ecb.SetComponent(sortKey, bulletEntity, bulletTransform);
                }

                shootableData.remainShootCount --;
                shootableData.currentShootCooltime = shootableData.shootCooltime;
            }
            
            if (shootableData.remainShootCount == 0 && shootableData.currentReloadTime <= 0f)
            {
                shootableData.currentReloadTime = shootableData.reloadTime;
            }
            
            refShootableData = shootableData;
        }
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        new ShootableJob()
        {
            ecb = ecb,
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();
    }
}
