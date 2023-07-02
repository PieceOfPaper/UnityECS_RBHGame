using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ECSBindProcessSystemGroup))]
public partial struct ECSAnimatorUpdateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (refMoveData, refTransform, bindAnimator) in SystemAPI.Query<RefRO<ECSMoveData>, RefRO<LocalTransform>, ECSBindAnimator>())
        {
            if (bindAnimator == null || bindAnimator.animator == null) continue;

            var moveData = refMoveData.ValueRO;
            bindAnimator.animator.SetBool("ECSMoveData.isMoving", moveData.isMoving);
            if (moveData.useCustomdir)
            {
                var dir = math.mul(refTransform.ValueRO.Rotation, moveData.customDir);
                bindAnimator.animator.SetFloat("ECSMoveData.moveDirX", dir.x);
                bindAnimator.animator.SetFloat("ECSMoveData.moveDirZ", dir.z);
            }
            else
            {
                var dir = math.mul(refTransform.ValueRO.Rotation, new float3(0f, 0f, 1f));
                bindAnimator.animator.SetFloat("ECSMoveData.moveDirX", dir.x);
                bindAnimator.animator.SetFloat("ECSMoveData.moveDirZ", dir.z);
            }
        }
        foreach (var (refShootableData, bindAnimator) in SystemAPI.Query<RefRO<ECSShootableData>, ECSBindAnimator>())
        {
            if (bindAnimator == null || bindAnimator.animator == null) continue;

            var shootableData = refShootableData.ValueRO;
            bindAnimator.animator.SetBool("ECSShootableData.pressShoot", shootableData.pressShoot);
        }
    }
}
