using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ECSCharacterAuthoring : MonoBehaviour
{
     public class Baker : Baker<ECSCharacterAuthoring>
     {
          public override void Bake(ECSCharacterAuthoring authoring)
          {
               var entity = GetEntity(TransformUsageFlags.Dynamic);
               AddComponent(entity, new ECSCharacterData());
          }
     }
}

