using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct ECSBulletData : IComponentData
{
    public float radius;
    public int attackableLayer;
    public int attackDamage;
    
    public bool isHit;
}
