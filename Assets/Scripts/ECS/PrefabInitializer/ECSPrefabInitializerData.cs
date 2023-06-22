using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

[ChunkSerializable]
public partial class ECSPrefabInitializerData : IComponentData/*, IEquatable<ECSPrefabInitializerData>, IDisposable*/
{
    public NativeHashMap<int, Entity> prefabs;
    
    public ECSPrefabInitializerData()
    {
        this.prefabs = new NativeHashMap<int, Entity>(0, Allocator.Persistent);
    }

    public ECSPrefabInitializerData (NativeHashMap<int, Entity> prefabs)
    {
        this.prefabs = prefabs;
    }
    
    public Entity GetPrefab(int id)
    {
        Entity prefab = default;
        if (this.prefabs.TryGetValue(id, out prefab) == false)
        {
            Debug.LogError("Not Found " + id);
            Debug.Log(this.prefabs.Count);
        }
        return prefab;
    }
    
    public Entity GetPrefab(string name)
    {
        return GetPrefab(ECSPrefabInitializerUtility.NameToID(name));
    }
}
