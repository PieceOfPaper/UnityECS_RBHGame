using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial class ECSBindAutoPrefab : IComponentData, IDisposable, ICloneable
{
    public GameObject prefab;
    public GameObject gameObject;
    
    public void Dispose()
    {
        if (gameObject != null)
        {
            if (Application.isPlaying)
                GameObject.Destroy(gameObject);
            else
                GameObject.DestroyImmediate(gameObject);
        }
        gameObject = null;
    }

    public object Clone()
    {
        return new ECSBindAutoPrefab()
        {
            prefab = prefab,
            gameObject = gameObject == null ? null : GameObject.Instantiate(prefab),
        };
    }
}
