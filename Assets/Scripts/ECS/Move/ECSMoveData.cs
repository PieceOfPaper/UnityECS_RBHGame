using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public partial struct ECSMoveData : IComponentData
{
    public bool useCustomdir;
    public float3 customDir;
    
    public float speed;
    public float accel;
    public float maxSpeed;
    
    public bool isMoving;
    public float currentSpeed;
}
