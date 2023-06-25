using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSSimpleAIAuthoring : MonoBehaviour
{
    public class Baker : Baker<ECSSimpleAIAuthoring>
    {
        public override void Bake(ECSSimpleAIAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSSimpleAIData()
            {
            });
        }
    }
}
