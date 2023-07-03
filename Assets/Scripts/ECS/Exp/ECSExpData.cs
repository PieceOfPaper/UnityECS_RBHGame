using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct ECSExpData : IComponentData
{
    public int exp;

    public bool isGathering;
    public float3 gatheringStartPos;
    public float gateringRate;
}
