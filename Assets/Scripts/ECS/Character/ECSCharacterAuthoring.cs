using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ECSCharacterAuthoring : MonoBehaviour
{
     public int hp = 100;
     public float radius = 1.0f;
     public ECSCharacterLayer layer;
    
     public int attackableLayer = 0;
     public int attackDamage = 0;
     
     public class Baker : Baker<ECSCharacterAuthoring>
     {
          public override void Bake(ECSCharacterAuthoring authoring)
          {
               var entity = GetEntity(TransformUsageFlags.Dynamic);
               AddComponent(entity, new ECSCharacterData()
               {
                    hp = authoring.hp,
                    maxHp = authoring.hp,
                    radius = authoring.radius,
                    layer = authoring.layer,
                    attackableLayer = authoring.attackableLayer,
                    attackDamage = authoring.attackDamage,
               });
          }
     }
     
#if UNITY_EDITOR

     private void OnDrawGizmos()
     {
          Gizmos.matrix = transform.localToWorldMatrix;
          Gizmos.color = Color.green;
          Gizmos.DrawWireSphere(Vector3.zero, radius);
     }

#endif
}

