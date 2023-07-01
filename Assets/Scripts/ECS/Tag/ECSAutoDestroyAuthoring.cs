using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSAutoDestroyAuthoring : MonoBehaviour
{
    public float delayTime = 1.0f;
    
    public partial class Baker : Baker<ECSAutoDestroyAuthoring>
    {
        public override void Bake(ECSAutoDestroyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSAutoDestroy()
            {
                delayTime = authoring.delayTime,
            });
        }
    }
}
