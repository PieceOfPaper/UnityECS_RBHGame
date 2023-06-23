using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSPlayerTagAuthoring : MonoBehaviour
{
    public class Baker : Baker<ECSPlayerTagAuthoring>
    {
        public override void Bake(ECSPlayerTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSPlayerTag());
        }
    }
}
