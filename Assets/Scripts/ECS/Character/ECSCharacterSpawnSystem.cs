using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;


public partial struct ECSCharacterSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }
    
    public void OnUpdate(ref SystemState state)
    {
        // var query = state.EntityManager.CreateEntityQuery(typeof(ECSPrefabInitializerData));
        // if (query.IsEmpty == false)
        // {
        //     var entity = query.GetSingletonEntity();
        //     var prefabInitializer = state.EntityManager.GetSharedComponent<ECSPrefabInitializerData>(entity);
        //     
        //     var prefab = prefabInitializer.GetEntity("Cube");
        //     var newEntity = state.EntityManager.Instantiate(prefab);
        //     state.EntityManager.SetComponentData(newEntity, new LocalTransform()
        //     {
        //         Position = new float3( UnityEngine.Random.Range(-100f, 100f), 0f, UnityEngine.Random.Range(-100f, 100f) ),
        //         Rotation = quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f),
        //         Scale = 1f,
        //     });
        // }
    }
}
