using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[ChunkSerializable]
public partial struct ECSPrefabInitializerData : ISharedComponentData, IEquatable<ECSPrefabInitializerData>
{
    private static int s_CurrentCode = 1;

    public int code;
    public NativeHashMap<int, Entity> datas;

    public ECSPrefabInitializerData (NativeArray<int> ids, NativeArray<Entity> entities)
    {
        code = s_CurrentCode ++;
        
        var cnt = math.min(ids.Length, entities.Length);
        this.datas = new NativeHashMap<int, Entity>(cnt, Allocator.Persistent);
        
        for (int i = 0; i < cnt; i ++)
        {
            this.datas.TryAdd(ids[i], entities[i]);
        }
    }
    
    public Entity GetEntity(int id)
    {
        Entity entity = default;
        this.datas.TryGetValue(id, out entity);
        return entity;
    }
    
    public Entity GetEntity(string name)
    {
        return GetEntity(ECSPrefabInitializerUtility.NameToID(name));
    }

    public bool Equals(ECSPrefabInitializerData other)
    {
        return this.code == other.code;
    }

    public override int GetHashCode()
    {
        return this.code;
    }
}
