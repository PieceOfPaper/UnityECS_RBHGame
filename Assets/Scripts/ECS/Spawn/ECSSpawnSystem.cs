using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public partial struct ECSSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    private partial struct SpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;
        
        private void Execute(ref ECSSpawnData refSpawnData, ref LocalTransform refTransform)
        {
            var spawnData = refSpawnData;
            if (spawnData.isEnable == false) return;

            if (spawnData.delayTimer > 0f)
            {
                spawnData.delayTimer -= deltaTime;
                refSpawnData = spawnData;
                return;
            }
            
            if (spawnData.spawnTimer > 0f)
            {
                spawnData.spawnTimer -= deltaTime;
                refSpawnData = spawnData;
                return;
            }

            if (spawnData.spawnCount > 0 && spawnData.spawnedCount >= spawnData.spawnCount)
            {
                return;
            }

            var random = spawnData.random;
            var rateSum = 0;
            rateSum += spawnData.EntityData1.rate;
            rateSum += spawnData.EntityData2.rate;
            rateSum += spawnData.EntityData3.rate;
            
            bool isFound = false;
            Entity foundEntity = default;
            var randRate = random.NextInt();
            var currentRate = 0;
            currentRate += spawnData.EntityData1.rate;
            if (isFound == false && currentRate >= randRate)
            {
                foundEntity = spawnData.EntityData1.entity;
                isFound = true;
            }
            currentRate += spawnData.EntityData2.rate;
            if (isFound == false && currentRate >= randRate)
            {
                foundEntity = spawnData.EntityData2.entity;
                isFound = true;
            }
            currentRate += spawnData.EntityData3.rate;
            if (isFound == false && currentRate >= randRate)
            {
                foundEntity = spawnData.EntityData3.entity;
                isFound = true;
            }

            if (isFound == true)
            {
                float3 pos = refTransform.Position;
                quaternion rot = quaternion.identity;
                switch (spawnData.rangeType)
                {
                    case ECSSpawnRangeType.Circle:
                        {
                            var eulerAngle = refTransform.Rotation.ToEuler();
                            var radiusRand = random.NextFloat();
                            var radius = (1f - radiusRand * radiusRand) * spawnData.rangeArg1;
                            var rangeAngle = random.NextFloat() * spawnData.rangeArg2;
                            pos += math.mul(Quaternion.Euler(0f, eulerAngle.y * Mathf.Rad2Deg + rangeAngle, 0f), new float3(0f, 0f, radius));
                        }
                        break;
                    case ECSSpawnRangeType.Box:
                        {
                            pos += math.mul(refTransform.Rotation, new float3(random.NextFloat() * spawnData.rangeArg1, 0f, random.NextFloat() * spawnData.rangeArg2));
                        }
                        break;
                }
                pos.y = 0f;
                rot = quaternion.Euler(0f, random.NextFloat() * 2.0f * math.PI, 0f);
                
                var newEntity = ecb.Instantiate(0, foundEntity);
                ecb.SetComponent(0, newEntity, new LocalTransform()
                {
                    Position = pos,
                    Rotation = rot,
                    Scale = 1f,
                });
                spawnData.spawnedCount ++;
                spawnData.spawnTimer += spawnData.spawnDelay;
            }

            spawnData.random = random;
            refSpawnData = spawnData;
        }
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        new SpawnJob()
        {
            ecb = ecb,
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();
    }
}
