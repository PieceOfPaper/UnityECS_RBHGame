using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ECSInputSystemGroup))]
public partial struct ECSSimpleAISystem : ISystem
{
    [BurstCompile]
    public partial struct SimpleAIMoveJob : IJobEntity
    {
        public bool foundPlayer;
        public LocalTransform playerTransform;
        
        private void Execute(in ECSSimpleAIData refAIData, ref ECSMoveData refMoveData, ref LocalTransform refTransformData)
        {
            var moveData = refMoveData;
            var transformData = refTransformData;
            if (foundPlayer == true)
            {
                var posDif = playerTransform.Position - transformData.Position;
                var posDifNormal = math.normalize(posDif);
                
                var radian = 90f * Mathf.Deg2Rad - math.atan2(posDifNormal.z, posDifNormal.x);
                transformData.Rotation = quaternion.Euler(0f, radian, 0f);
                
                moveData.isMoving = true;
            }
            else
            {
                moveData.isMoving = false;
            }
            refMoveData = moveData;
            refTransformData = transformData;
        }
    }
    
    [BurstCompile]
    public partial struct SimpleAIShootJob : IJobEntity
    {
        private void Execute(in ECSSimpleAIData refAIData, ref ECSShootableData refShootableData)
        {
            var shootableData = refShootableData;
            shootableData.pressShoot = true;
            refShootableData = shootableData;
        }
    }
    
    public void OnUpdate(ref SystemState state)
    {
        bool foundPlayer = false;
        LocalTransform playerTransform = default;
        foreach (var (playerTag, localTransform) in SystemAPI.Query<RefRO<ECSPlayerTag>, RefRO<LocalTransform>>())
        {
            foundPlayer = true;
            playerTransform = localTransform.ValueRO;
            break;
        }

        var simpleAIMoveJobHandle = new SimpleAIMoveJob()
        {
            foundPlayer = foundPlayer,
            playerTransform = playerTransform,
        };
        state.Dependency = simpleAIMoveJobHandle.ScheduleParallel(state.Dependency);
        var simpleAIShootJobHandle = new SimpleAIShootJob()
        {
        };
        state.Dependency = simpleAIShootJobHandle.ScheduleParallel(state.Dependency);
    }
}
