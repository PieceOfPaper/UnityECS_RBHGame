using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSBindGameObject : IComponentData, IDisposable, ICloneable
{
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
        return gameObject == null ? null : GameObject.Instantiate(gameObject);
    }
}
