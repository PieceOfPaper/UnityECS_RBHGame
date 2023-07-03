using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public partial struct ECSExpGatherSystem : ISystem
{
    
    [BurstCompile]
    private partial struct GatheringExpJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;
        public ECSPlayerData playerData;
        public float3 playerPosition;
        public NativeArray<int> resultExp;

        private void Execute([EntityIndexInQuery] int index, [ChunkIndexInQuery] int sortKey, in Entity refEntity, ref ECSExpData refExpData, ref LocalTransform refTransform)
        {
            var expData = refExpData;
            var transform = refTransform;
            if (expData.isGathering)
            {
                if (expData.gateringRate < 1f)
                {
                    expData.gateringRate += deltaTime * 4.0f; //gathering speed
                    if (expData.gateringRate > 1f)
                    {
                        expData.gateringRate = 1f;
                        resultExp[index] = expData.exp;
                        ecb.DestroyEntity(sortKey, refEntity);
                    }
                    else
                    {
                        transform.Position = math.lerp(expData.gatheringStartPos, playerPosition, expData.gateringRate);
                    }
                }
            }
            else
            {
                var expPos = transform.Position;
                var distanceSqr = math.distancesq(new float2(expPos.x, expPos.z), new float2(playerPosition.x, playerPosition.z));
                if (distanceSqr < playerData.expGatherRadius * playerData.expGatherRadius)
                {
                    expData.gatheringStartPos = expPos;
                    expData.isGathering = true;
                }
            }
            refExpData = expData;
            refTransform = transform;
        }
    }
    
    [BurstCompile]
    private partial struct PlayerGatherExpJob : IJobEntity
    {
        [ReadOnly] public NativeArray<int> resultExp;

        private void Execute(ref ECSPlayerData refPlayerData)
        {
            var playerData = refPlayerData;
            for (int i = 0; i < resultExp.Length; i ++)
            {
                playerData.exp += resultExp[i];
            }
            refPlayerData = playerData;
        }
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        
        var playerQuery = state.EntityManager.CreateEntityQuery(typeof(ECSPlayerData), typeof(LocalTransform));
        var playerDataArray = playerQuery.ToComponentDataArray<ECSPlayerData>(Allocator.Temp);
        var playerTransformArray = playerQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        var playerData = playerDataArray.Length > 0 ? playerDataArray[0] : default;
        var playerTransform = playerTransformArray.Length > 0 ? playerTransformArray[0] : default;
        playerDataArray.Dispose();
        playerTransformArray.Dispose();
        
        var expQuery = state.EntityManager.CreateEntityQuery(typeof(ECSExpData), typeof(LocalTransform));
        var expCount = expQuery.CalculateEntityCount();
        var resultExp = new NativeArray<int>(expCount, Allocator.TempJob);
        var gatheringExpJobHandle = new GatheringExpJob()
        {
            ecb = ecb,
            deltaTime = SystemAPI.Time.DeltaTime,
            playerData = playerData,
            playerPosition = playerTransform.Position,
            resultExp = resultExp,
        };
        state.Dependency = gatheringExpJobHandle.ScheduleParallel(expQuery, state.Dependency);
        var playerGatherExpJobHandle = new PlayerGatherExpJob()
        {
            resultExp = resultExp,
        };
        state.Dependency = playerGatherExpJobHandle.ScheduleParallel(playerQuery, state.Dependency);
        state.Dependency = resultExp.Dispose(state.Dependency);
    }
}
