using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSExpAuthoring : MonoBehaviour
{
    public int exp = 1;
    
    public class Baker : Baker<ECSExpAuthoring>
    {
        public override void Bake(ECSExpAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSExpData()
            {
                exp = authoring.exp,
            });
        }
    }
}
