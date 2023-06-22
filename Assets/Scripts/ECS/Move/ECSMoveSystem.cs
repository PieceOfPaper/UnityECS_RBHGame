using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct ECSMoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // throw new System.NotImplementedException();
    }

    public void OnDestroy(ref SystemState state)
    {
        // throw new System.NotImplementedException();
    }

    [BurstCompile]
    private partial struct MoveJob : IJobEntity
    {
        public float deltaTime;
        
        private void Execute(ref ECSMoveData moveData, ref LocalTransform transform)
        {
            var dir = moveData.customDir;
            if (moveData.useCustomdir == false)
                dir = math.mul(transform.Rotation, new float3(0f, 0f, 1f));

            var moveDistance = moveData.currentSpeed * deltaTime + 0.5f * moveData.accel * deltaTime * deltaTime;
            moveData.currentSpeed = math.min(moveData.currentSpeed + moveData.accel * deltaTime, moveData.maxSpeed);
            
            transform = transform.Translate(dir * moveDistance);
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        new MoveJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();
    }
}
