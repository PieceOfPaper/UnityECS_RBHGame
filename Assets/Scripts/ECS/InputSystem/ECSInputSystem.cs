using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(ECSInputSystemGroup))]
public partial struct ECSInputSystem : ISystem, InputActionMain.IPlayerActions
{
    static InputActionMain mainInputAction = null;
    static bool onKeyFire = false;
    static Vector2 moveDir = Vector2.zero;
    
    public void OnCreate(ref SystemState state)
    {
        mainInputAction = new InputActionMain();
        mainInputAction.Enable();
        mainInputAction.Player.SetCallbacks(this);
    }

    public void OnDestroy(ref SystemState state)
    {
        if (mainInputAction != null)
        {
            mainInputAction.Dispose();
            mainInputAction = null;
        }
        
        onKeyFire = false;
        moveDir = Vector2.zero;
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerTag, moveDataRW, localTransform) in SystemAPI.Query<RefRO<ECSPlayerTag>, RefRW<ECSMoveData>, RefRW<LocalTransform>>())
        {
            var moveData = moveDataRW.ValueRO;
            if (moveDir.sqrMagnitude > 0f)
            {
                var radian = 90f * Mathf.Deg2Rad - math.atan2(moveDir.y, moveDir.x);
                localTransform.ValueRW.Rotation = quaternion.Euler(0f, radian, 0f);
                moveData.isMoving = true;
            }
            else
            {
                moveData.isMoving = false;
            }
            moveDataRW.ValueRW = moveData;
        }
        foreach (var (playerTag, shootableDataRW, localTransform) in SystemAPI.Query<RefRO<ECSPlayerTag>, RefRW<ECSShootableData>, RefRW<LocalTransform>>())
        {
            var shootableData = shootableDataRW.ValueRO;
            shootableData.pressShoot = onKeyFire;
            shootableDataRW.ValueRW = shootableData;
        }
    }
    

    void InputActionMain.IPlayerActions.OnFire(InputAction.CallbackContext context)
    {
        onKeyFire = context.ReadValueAsButton();
    }

    void InputActionMain.IPlayerActions.OnLook(InputAction.CallbackContext context)
    {
        // throw new System.NotImplementedException();
    }
    
    void InputActionMain.IPlayerActions.OnMove(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
    }
}
