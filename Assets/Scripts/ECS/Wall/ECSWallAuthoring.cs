using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSWallAuthoring : MonoBehaviour
{
    public ECSRangeType rangeType = ECSRangeType.Circle;
    public float rangeArg1 = 1.0f;
    public float rangeArg2 = 360.0f;
    
    public class Baker : Baker<ECSWallAuthoring>
    {
        public override void Bake(ECSWallAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSWallData()
            {
                rangeType = authoring.rangeType,
                rangeArg1 = authoring.rangeArg1,
                rangeArg2 = authoring.rangeArg2,
            });
            
            
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        switch (rangeType)
        {
            case ECSRangeType.Circle:
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
                    Gizmos.DrawWireMesh(circleMesh, Vector3.zero, Quaternion.identity);
                }
                break;
            case ECSRangeType.Box:
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(rangeArg1, 0.0f, rangeArg2));
                break;
        }
    }
#endif
}
