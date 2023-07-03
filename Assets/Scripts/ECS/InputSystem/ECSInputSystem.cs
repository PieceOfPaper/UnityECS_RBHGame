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
    private static InputActionMain mainInputAction = null;
    private static bool onKeyFire = false;
    private static Vector2 moveDir = Vector2.zero;
    private static Vector2 lookDir = Vector2.zero;
    
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
        var mainCamera = Camera.main;
        foreach (var (playerTag, moveDataRW, shootableDataRW, localTransform) in SystemAPI.Query<RefRO<ECSPlayerData>, RefRW<ECSMoveData>, RefRW<ECSShootableData>, RefRW<LocalTransform>>())
        {
            if (lookDir.sqrMagnitude > 0f)
            {
                localTransform.ValueRW.Rotation = quaternion.Euler(0f, 90f * Mathf.Deg2Rad - math.atan2(lookDir.y, lookDir.x), 0f);
            }
            
            var moveData = moveDataRW.ValueRO;
            if (moveDir.sqrMagnitude > 0f)
            {
                moveData.useCustomdir = true;
                moveData.customDir = mainCamera == null ? new float3(moveDir.x, 0f, moveDir.y) : math.mul(quaternion.Euler(0f, mainCamera.transform.eulerAngles.y * Mathf.Deg2Rad, 0f), new float3(moveDir.x, 0f, moveDir.y));
                moveData.isMoving = true;
            }
            else
            {
                moveData.useCustomdir = false;
                moveData.isMoving = false;
            }
            moveDataRW.ValueRW = moveData;
            
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
        switch (context.control.device.name)
        {
            case "Mouse":
                lookDir = Input.mousePosition - new Vector3(Screen.width, Screen.height) * 0.5f;
                break;
            default:
                lookDir = context.ReadValue<Vector2>();
                break;
        }
    }
    
    void InputActionMain.IPlayerActions.OnMove(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
    }
}
