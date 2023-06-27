using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ECSAfterProcessSystemGroup))]
public partial struct ECSBindPrefabSystem : ISystem
{
    private static Dictionary<GameObject, Queue<GameObject>> m_QueuedAutoPrefabs;

    public void OnCreate(ref SystemState state)
    {
        m_QueuedAutoPrefabs = new Dictionary<GameObject, Queue<GameObject>>();
    }

    public void OnDestroy(ref SystemState state)
    {
        foreach (var queue in m_QueuedAutoPrefabs.Values)
        {
            while (queue.Count > 0)
            {
                var obj = queue.Dequeue();
                if (obj == null) continue;
                GameObject.Destroy(obj);
            }
        }
        m_QueuedAutoPrefabs.Clear();
    }

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

        var mainCamera = Camera.main;
        foreach (var (localTransform, bindAutoPrefab, entity) in SystemAPI.Query<RefRO<LocalTransform>, ECSBindAutoPrefab>().WithEntityAccess())
        {
            var viewportPoint = mainCamera == null ? Vector3.one * 10f : mainCamera.WorldToViewportPoint(localTransform.ValueRO.Position);
            if (viewportPoint.x > -0.125f && viewportPoint.x < 1.125f && viewportPoint.y > -0.125f && viewportPoint.y < 1.125f)
            {
                if (bindAutoPrefab.gameObject == null && bindAutoPrefab.prefab != null)
                {
                    GameObject gameObject = null;
                    if (m_QueuedAutoPrefabs.ContainsKey(bindAutoPrefab.prefab) && m_QueuedAutoPrefabs[bindAutoPrefab.prefab].Count > 0)
                    {
                        gameObject = m_QueuedAutoPrefabs[bindAutoPrefab.prefab].Dequeue();
                        gameObject.SetActive(true);
                    }
                    else
                    {
                        gameObject = GameObject.Instantiate(bindAutoPrefab.prefab);
                    }
                    
                    bindAutoPrefab.gameObject = gameObject;
                    ecb.SetComponent(entity, new ECSBindTransform()
                    {
                        transform = gameObject.transform,
                    });
                }
            }
            else
            {
                if (bindAutoPrefab.gameObject != null)
                {
                    if (m_QueuedAutoPrefabs.ContainsKey(bindAutoPrefab.prefab) == false)
                        m_QueuedAutoPrefabs.Add(bindAutoPrefab.prefab, new Queue<GameObject>());
                    
                    bindAutoPrefab.gameObject.gameObject.SetActive(false);
                    m_QueuedAutoPrefabs[bindAutoPrefab.prefab].Enqueue(bindAutoPrefab.gameObject);
                    
                    bindAutoPrefab.gameObject = null;
                    ecb.SetComponent(entity, new ECSBindTransform()
                    {
                        transform = null,
                    });
                }
            }
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
