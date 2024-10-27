using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MathHelpers
{
    public static float GetHeading(float3 objPos, float3 targetPos)
    {
        var x = objPos.x - targetPos.x;
        var y = objPos.z - targetPos.z;

        return math.atan2(x, y) + math.PI;
    }


    public static float GetDistance(float3 positionA, float3 positionB)
    {
        return Vector3.Distance(positionA, positionB);
    }
}
