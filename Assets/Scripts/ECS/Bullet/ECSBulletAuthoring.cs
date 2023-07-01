using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSBulletAuthoring : MonoBehaviour
{
    public float radius = 0.2f;
    public int attackableLayer;
    public int attackDamage;
    
    public int hitCount = 1;
    public float moveDistance = 10f;
    public float duration = 1.0f;

    public GameObject hitEffect;
    
    public partial class Baker : Baker<ECSBulletAuthoring>
    {
        public override void Bake(ECSBulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSBulletData()
            {
                radius = authoring.radius,
                attackableLayer = authoring.attackableLayer,
                attackDamage = authoring.attackDamage,
                hitCount = authoring.hitCount,
                moveDistance = authoring.moveDistance,
                duration = authoring.duration,
                hitEffect = GetEntity(authoring.hitEffect, TransformUsageFlags.Dynamic),
            });
        }
    }
}
