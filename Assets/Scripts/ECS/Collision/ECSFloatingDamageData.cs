using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct ECSFloatingDamageData : IComponentData
{
    public ECSCharacterLayer layer;
    public float3 position;
    public int damage;
}
