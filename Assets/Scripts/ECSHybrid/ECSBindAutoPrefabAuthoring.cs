using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSBindAutoPrefabAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public class Baker : Baker<ECSBindAutoPrefabAuthoring>
    {
        public override void Bake(ECSBindAutoPrefabAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new ECSBindAutoPrefab()
            {
                prefab = authoring.prefab,
                gameObject = null,
            });
            AddComponentObject(entity, new ECSBindTransform()
            {
                transform = null,
            });
            AddComponentObject(entity, new ECSBindAnimator()
            {
                animator = null,
            });
        }
    }
}
