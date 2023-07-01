using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(ECSProcessSystemGroup))]
public partial struct ECSGlobalProcessSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        foreach (var (refAutoDestroy, entity) in SystemAPI.Query<RefRW<ECSAutoDestroy>>().WithEntityAccess())
        {
            var autoDestroy = refAutoDestroy.ValueRO;
            if (autoDestroy.currentDelayTime < autoDestroy.delayTime)
            {
                autoDestroy.currentDelayTime += SystemAPI.Time.DeltaTime;
                refAutoDestroy.ValueRW = autoDestroy;
            }
            else
            {
                ecb.DestroyEntity(0, entity);
            }
        }
    }
}
