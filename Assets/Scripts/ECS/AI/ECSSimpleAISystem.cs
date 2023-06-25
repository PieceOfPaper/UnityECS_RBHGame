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
    public partial struct SimpleAIJob : IJobEntity
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

        new SimpleAIJob()
        {
            foundPlayer = foundPlayer,
            playerTransform = playerTransform,
        }.ScheduleParallel();
    }
}
