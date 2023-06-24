using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(ECSAfterProcessSystemGroup))]
[UpdateAfter(typeof(ECSBindPrefabSystem))]
public partial struct ECSBindTransformSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transformData, bindTransform) in SystemAPI.Query<RefRO<LocalTransform>, ECSBindTransform>())
        {
            if (bindTransform == null || bindTransform.transform == null) continue;

            var data = transformData.ValueRO;
            bindTransform.transform.SetPositionAndRotation(data.Position, data.Rotation);
            bindTransform.transform.localScale = Vector3.one * data.Scale;
        }
    }
}
