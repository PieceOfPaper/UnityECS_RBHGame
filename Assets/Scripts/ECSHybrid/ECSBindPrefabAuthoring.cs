using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSBindPrefabAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public class Baker : Baker<ECSBindPrefabAuthoring>
    {
        public override void Bake(ECSBindPrefabAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new ECSBindPrefab()
            {
                prefab = authoring.prefab,
            });
        }
    }
}
