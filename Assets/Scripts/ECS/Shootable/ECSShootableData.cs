using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct ECSShootableData : IComponentData
{
    public Entity bullet;

    public float3 shootPoint;
    public int shootCount;
    public float shootSpreadRange;
    public float shootCooltime;
    
    public int reloadCount;
    public float reloadTime;

    public bool pressShoot;
    public int remainShootCount;
    public float currentShootCooltime;
    public float currentReloadTime;
}
