using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

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
            var ids = new NativeArray<int>(authoring.datas.Length, Allocator.Temp);
            var entities = new NativeArray<Entity>(authoring.datas.Length, Allocator.Temp);
            for (int i = 0; i < authoring.datas.Length; i ++)
            {
                ids[i] = ECSPrefabInitializerUtility.NameToID(authoring.datas[i].name);
                entities[i] = GetEntity(authoring.datas[i].prefab);
            }

            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var componentData = new ECSPrefabInitializerData(ids, entities);
            AddSharedComponentManaged(entity, componentData);

            ids.Dispose();
            entities.Dispose();
        }
    }
}
