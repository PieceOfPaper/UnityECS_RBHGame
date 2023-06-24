using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MathUtility
{
    /// <summary>
    /// Converts quaternion representation to euler
    /// </summary>
    public static float3 ToEuler(this quaternion quaternion)
    {
        float4 q = quaternion.value;
        double3 res;
 
        double sinr_cosp = +2.0 * (q.w * q.x + q.y * q.z);
        double cosr_cosp = +1.0 - 2.0 * (q.x * q.x + q.y * q.y);
        res.x = math.atan2(sinr_cosp, cosr_cosp);
 
        double sinp = +2.0 * (q.w * q.y - q.z * q.x);
        if (math.abs(sinp) >= 1) {
            res.y = math.PI / 2 * math.sign(sinp);
        } else {
            res.y = math.asin(sinp);
        }
 
        double siny_cosp = +2.0 * (q.w * q.z + q.x * q.y);
        double cosy_cosp = +1.0 - 2.0 * (q.y * q.y + q.z * q.z);
        res.z = math.atan2(siny_cosp, cosy_cosp);
 
        return (float3) res;
    }
}
