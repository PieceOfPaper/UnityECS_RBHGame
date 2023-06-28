using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSShootableAuthoring : MonoBehaviour
{
    public GameObject bullet;

    public Vector3 shootPoint;
    public int shootCount = 1;
    public float shootSpreadRange = 10f;
    public float shootCooltime = 0.5f;
    
    public int reloadCount = 6;
    public float reloadTime = 1.0f;

    public partial class Baker : Baker<ECSShootableAuthoring>
    {
        public override void Bake(ECSShootableAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSShootableData()
            {
                bullet = GetEntity(authoring.bullet, TransformUsageFlags.Dynamic),
                shootPoint = authoring.shootPoint,
                shootCount = authoring.shootCount,
                shootSpreadRange = authoring.shootSpreadRange,
                shootCooltime = authoring.shootCooltime,
                reloadCount = authoring.reloadCount,
                reloadTime = authoring.reloadTime,
                currentReloadTime = authoring.reloadTime,
                remainShootCount = 0,
            });
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, shootPoint);
        Gizmos.DrawSphere(shootPoint, 0.1f);
    }
#endif
    
}
