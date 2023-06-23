using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ECSCharacterAuthoring : MonoBehaviour
{
     public int hp = 100;
     public GameObject prefab;
     
     public class Baker : Baker<ECSCharacterAuthoring>
     {
          public override void Bake(ECSCharacterAuthoring authoring)
          {
               var entity = GetEntity(TransformUsageFlags.Dynamic);
               AddComponent(entity, new ECSCharacterData()
               {
                    hp = authoring.hp,
                    maxHp = authoring.hp,
               });
               
               var gameObject = GameObject.Instantiate(authoring.prefab);
               AddComponentObject(entity, new ECSBindGameObject()
               {
                    gameObject = gameObject,
               });
               AddComponentObject(entity, new ECSBindTransform()
               {
                    transform = gameObject.transform,
               });
          }
     }
}

