﻿using UnityEngine;

namespace Tools
{
    public static class MathTools
    {
        public static Vector3 GetYRotatedPoint(Vector3 point, Vector3 relativePoint, float cosinus, float sinus)
        {
            Vector3 direction = point - relativePoint;
            return new Vector3(
                direction.x * cosinus + direction.z * sinus + relativePoint.x,
                point.y,
                -direction.x * sinus + direction.z * cosinus + relativePoint.z);
        }
    }
}