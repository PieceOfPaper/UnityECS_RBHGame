using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Entities.Serialization;
using Unity.Scenes;

public static class ECSPrefabInitializerUtility
{
    private static int s_CurrentID = 1;
    private static Dictionary<string, int> s_NameToID = new Dictionary<string, int>();

    public static int NameToID(string name)
    {
        if (s_NameToID.ContainsKey(name) == false)
        {
            s_NameToID.Add(name, s_CurrentID ++);
        }

        return s_NameToID[name];
    }
    
}

public class ECSPrefabInitializerAuthoring : MonoBehaviour
{
    [SerializeField] public Data[] datas;
    
    [System.Serializable]
    public struct Data
    {
        public string name;
        public GameObject prefab;
    }

    public class Baker : Baker<ECSPrefabInitializerAuthoring>
    {
        public override void Bake(ECSPrefabInitializerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var datas = new NativeHashMap<int, Entity>(authoring.datas.Length, Allocator.Persistent);
            for (int i = 0; i < authoring.datas.Length; i ++)
            {
                var id = ECSPrefabInitializerUtility.NameToID(authoring.datas[i].name);
                var prefab = GetEntity(authoring.datas[i].prefab, TransformUsageFlags.Dynamic);
                datas.TryAdd(id, prefab);
            }

            var componentData = new ECSPrefabInitializerData(datas);
            AddComponentObject(entity, componentData);
        }
    }
}
