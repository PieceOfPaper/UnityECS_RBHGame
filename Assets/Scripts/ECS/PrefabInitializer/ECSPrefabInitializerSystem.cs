using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Scenes;

public partial struct ECSPrefabInitializerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // throw new System.NotImplementedException();
    }

    public void OnUpdate(ref SystemState state)
    {
        // var ecb = new EntityCommandBuffer(Allocator.Temp);
        // foreach (var (prefabInitializer, prefabLoadResult, entity) in SystemAPI.Query<ECSPrefabInitializerData, RefRO<PrefabLoadResult>>().WithEntityAccess())
        // {
        //     prefabInitializer.AddLoadedEntity(prefabLoadResult.ValueRO.PrefabRoot);
        //     ecb.RemoveComponent(entity, typeof(PrefabLoadResult));
        //     if (prefabInitializer.loadingID != 0)
        //     {
        //         ecb.AddComponent(entity, new RequestEntityPrefabLoaded() { Prefab = prefabInitializer.GetPrefabReference(prefabInitializer.loadingID) });
        //     }
        //     Debug.Log(prefabInitializer.loadingID);
        // }
        // ecb.Playback(state.EntityManager);
        // ecb.Dispose();
    }
}
