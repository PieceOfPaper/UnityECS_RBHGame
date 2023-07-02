using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct ECSBulletData : IComponentData
{
    public float radius;
    public int attackableLayer;
    public int attackDamage;
    public int hitCount;
    public float duration;
    public Entity hitEffect;
    
    public float currentTime;
    public int currentHitCount;

    public bool IsWillDestroy()
    {
        return (hitCount > 0 && currentHitCount >= hitCount) || (duration > 0f && currentTime >= duration);
    }
}
