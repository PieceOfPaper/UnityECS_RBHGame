using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSPlayerAuthoring : MonoBehaviour
{
    public float expGatherRadius = 1.0f;
    
    public class Baker : Baker<ECSPlayerAuthoring>
    {
        public override void Bake(ECSPlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSPlayerData()
            {
                expGatherRadius = authoring.expGatherRadius,
                level = 1,
                exp = 0,
            });
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, expGatherRadius);
    }
#endif
    
}
