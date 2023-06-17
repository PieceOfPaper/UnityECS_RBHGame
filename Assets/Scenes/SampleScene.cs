using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

public class SampleScene : MonoBehaviour
{
    private CharacterResourceData m_CharResourceData;

    private Dictionary<Entity, GameObject> m_DicObjs = new Dictionary<Entity, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        m_CharResourceData = Resources.Load<CharacterResourceData>("CharacterResourceData");
    }

    // Update is called once per frame
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        {
            ECSCharacterSpawnManager.Instance.EnqueueSpawnData(new ECSCharacterSpawnData()
            {
                characterData = ECSCharacterData.Create(1, 100),
                transformData = new LocalTransform() { Position = new float3(), Rotation = quaternion.identity, Scale = 1f },
            });
        }
    }
    
    
    private struct TransformJob : IJobParallelForTransform
    {    
        [ReadOnly] public NativeArray<LocalTransform> transformDatas;
        
        public void Execute(int index, TransformAccess transform)
        {
            var transformData = transformDatas[index];
            transform.position = transformData.Position;
            transform.rotation = transformData.Rotation;
            transform.localScale = Vector3.one * transformData.Scale;
        }
    }

    private void LateUpdate()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(ECSCharacterData));
        if (query.IsEmpty == false)
        {
            var transformList = new List<Transform>();
            var transformDataList = new List<LocalTransform>();
            foreach (var entity in query.ToEntityArray(Allocator.Temp))
            {
                if (m_DicObjs.ContainsKey(entity) == false)
                {
                    var characterData = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<ECSCharacterData>(entity);
                    var prefabName = m_CharResourceData.GetPrefabName(characterData.resourceID);
                    var prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
                    var obj = Instantiate(prefab);
                    m_DicObjs.Add(entity, obj);
                }
                
                var transformData = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalTransform>(entity);

                // m_DicObjs[entity].transform.position = entityTransform.Position;
                // m_DicObjs[entity].transform.rotation = entityTransform.Rotation;
                // m_DicObjs[entity].transform.localScale = Vector3.one * entityTransform.Scale;
                
                transformList.Add(m_DicObjs[entity].transform);
                transformDataList.Add(transformData);
            }
            
            var accessArray = new TransformAccessArray(transformList.ToArray());
            var transformDatas = transformDataList.ToNativeArray(Allocator.TempJob);

            var job = new TransformJob()
            {
                transformDatas = transformDatas,
            };
            
            var jobHandle = job.Schedule(accessArray);
            jobHandle.Complete();
            
            transformDatas.Dispose();
            accessArray.Dispose();
        }
    }
}
