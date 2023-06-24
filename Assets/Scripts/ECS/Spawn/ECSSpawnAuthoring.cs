using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ECSSpawnAuthoring : MonoBehaviour
{
    public bool isEnable = true;
    public int spawnID = 0;
    public float delay = 1.0f;
    
    public EntityData[] spawnEntityDatas;
    public float spawnDelay = 0.5f;
    public int spawnCount = 1;

    public ECSSpawnRangeType rangeType = ECSSpawnRangeType.Circle;
    public float rangeArg1 = 1.0f;
    public float rangeArg2 = 360.0f;

    [System.Serializable]
    public struct EntityData
    {
        public GameObject prefab;
        public int rate;
    }
    
    public class Baker : Baker<ECSSpawnAuthoring>
    {
        public override void Bake(ECSSpawnAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSSpawnData()
            {
                isEnable = authoring.isEnable,
                spawnID = authoring.spawnID,
                delay = authoring.delay,
                EntityData1 = authoring.spawnEntityDatas != null && authoring.spawnEntityDatas.Length > 0 ? new ECSSpawnData.EntityData()
                {
                    entity = GetEntity(authoring.spawnEntityDatas[0].prefab, TransformUsageFlags.Dynamic),
                    rate = authoring.spawnEntityDatas[0].rate,
                } : default,
                EntityData2 = authoring.spawnEntityDatas != null && authoring.spawnEntityDatas.Length > 1 ? new ECSSpawnData.EntityData()
                {
                    entity = GetEntity(authoring.spawnEntityDatas[1].prefab, TransformUsageFlags.Dynamic),
                    rate = authoring.spawnEntityDatas[1].rate,
                } : default,
                EntityData3 = authoring.spawnEntityDatas != null && authoring.spawnEntityDatas.Length > 2 ? new ECSSpawnData.EntityData()
                {
                    entity = GetEntity(authoring.spawnEntityDatas[2].prefab, TransformUsageFlags.Dynamic),
                    rate = authoring.spawnEntityDatas[0].rate,
                } : default,
                spawnDelay = authoring.spawnDelay,
                spawnCount = authoring.spawnCount,
                rangeType = authoring.rangeType,
                rangeArg1 = authoring.rangeArg1,
                rangeArg2 = authoring.rangeArg2,
                delayTimer = authoring.delay,
                random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Millisecond),
            });
            
            
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        switch (rangeType)
        {
            case ECSSpawnRangeType.Circle:
                // Gizmos.DrawSphere(Vector3.zero, rangeArg1);
                var circleAngle = rangeArg2;
                if (circleAngle >= 0f)
                {
                    if (circleAngle > 360f) circleAngle = 360f;
                    var defaultUnit = 360f / 16f;
                    var pointCount = Mathf.RoundToInt(circleAngle / defaultUnit);
                    var circleAngleUnit = circleAngle / pointCount;

                    var vertexList = new List<Vector3>();
                    var normalList = new List<Vector3>();
                    var triangleList = new List<int>();
                    vertexList.Add(Vector3.zero);
                    normalList.Add(Vector3.up);
                    for (int pointIndex = 0; pointIndex < pointCount; pointIndex ++)
                    {
                        var angle = pointIndex * circleAngleUnit;
                        vertexList.Add(
                            new Vector3(
                                rangeArg1 * Mathf.Cos(angle * Mathf.Deg2Rad),
                            0f,
                                rangeArg1 * Mathf.Sin(angle * Mathf.Deg2Rad)));
                        normalList.Add(Vector3.up);
                    }
                    for (int pointIndex = 0; pointIndex < pointCount; pointIndex ++)
                    {
                        if ((pointIndex + 1) < pointCount)
                        {
                            triangleList.Add(0);
                            triangleList.Add(pointIndex + 2);
                            triangleList.Add(pointIndex + 1);
                        }
                        else if (circleAngle >= 360f)
                        {
                            triangleList.Add(0);
                            triangleList.Add(1);
                            triangleList.Add(pointIndex + 1);
                        }
                    }
                    
                    var circleMesh = new Mesh();
                    circleMesh.vertices = vertexList.ToArray();
                    circleMesh.normals = normalList.ToArray();
                    circleMesh.triangles = triangleList.ToArray();
                    Gizmos.DrawMesh(circleMesh, Vector3.zero, Quaternion.identity);
                }
                break;
            case ECSSpawnRangeType.Box:
                Gizmos.DrawCube(Vector3.zero, new Vector3(rangeArg1, 0.0f, rangeArg2));
                break;
        }
    }
#endif
}
