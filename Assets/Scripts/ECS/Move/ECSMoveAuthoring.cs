using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.UIElements;

public class ECSMoveAuthoring : MonoBehaviour
{
    public bool useCustomdir;
    public Vector3 customDir;
    
    public float speed;
    public float accel;
    public float maxSpeed;
    
    public class Baker : Baker<ECSMoveAuthoring>
    {
        public override void Bake(ECSMoveAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSMoveData()
            {
                useCustomdir = authoring.useCustomdir,
                customDir = authoring.customDir,
                speed = authoring.speed,
                accel = authoring.accel,
                maxSpeed = authoring.maxSpeed,
                currentSpeed = authoring.speed,
            });
        }
    }
}
