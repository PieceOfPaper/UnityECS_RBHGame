using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(ECSProcessSystemGroup))]
public partial struct ECSMoveSystem : ISystem
{
    [BurstCompile]
    private partial struct MoveJob : IJobEntity
    {
        public float deltaTime;
        
        private void Execute(ref ECSMoveData refMoveData, ref LocalTransform transform)
        {
            var moveData = refMoveData;
            
            float3 moveTranslation;
            if (moveData.isMoving == true)
            {
                var dir = moveData.customDir;
                if (moveData.useCustomdir == false)
                    dir = math.mul(transform.Rotation, new float3(0f, 0f, 1f));
                
                var moveDistance = moveData.currentSpeed * deltaTime + 0.5f * moveData.accel * deltaTime * deltaTime;
                moveData.currentSpeed = math.min(moveData.currentSpeed + moveData.accel * deltaTime, moveData.maxSpeed);
                moveTranslation = dir * moveDistance;
            }
            else
            {
                moveData.currentSpeed = 0f;
                moveTranslation = float3.zero;
            }

            transform = transform.Translate(moveTranslation + moveData.force * deltaTime);
            moveData.force *= math.max(0f, 1f - deltaTime);
            refMoveData = moveData;
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        var moveJob = new MoveJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        };
        state.Dependency = moveJob.ScheduleParallel(state.Dependency);
    }
}
