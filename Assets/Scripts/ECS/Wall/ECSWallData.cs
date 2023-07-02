using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct ECSWallData : IComponentData
{
    public ECSRangeType rangeType;
    public float rangeArg1;
    public float rangeArg2;
}

public readonly partial struct ECSWallAspect : IAspect
{
    public readonly Entity entity;
    private readonly RefRO<ECSWallData> wallData;
    private readonly RefRO<LocalTransform> transform;
}

