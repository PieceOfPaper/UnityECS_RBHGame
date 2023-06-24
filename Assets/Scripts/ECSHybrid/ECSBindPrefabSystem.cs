using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial struct ECSBindPrefabSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (bindPrefab, entity) in SystemAPI.Query<ECSBindPrefab>().WithEntityAccess())
        {
            if (bindPrefab.prefab != null)
            {
                var gameObject = GameObject.Instantiate(bindPrefab.prefab);
                ecb.AddComponent<ECSBindGameObject>(entity);
                ecb.SetComponent(entity, new ECSBindGameObject()
                {
                    gameObject = gameObject,
                });
                ecb.AddComponent<ECSBindTransform>(entity);
                ecb.SetComponent(entity, new ECSBindTransform()
                {
                    transform = gameObject.transform,
                });
            }
            
            ecb.RemoveComponent<ECSBindPrefab>(entity);
            
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
