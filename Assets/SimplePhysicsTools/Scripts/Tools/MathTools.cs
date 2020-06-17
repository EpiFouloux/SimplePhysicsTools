using UnityEngine;

namespace SimplePhysicsTools.Tools
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
        
        public static Vector3 GetBezierPoint (Vector3 start, Vector3 tangent, Vector3 end, float t) {
            return Vector3.Lerp(Vector3.Lerp(start, tangent, t), Vector3.Lerp(tangent, end, t), t);
        }

        public static Vector3[] GetBezierCurve(Vector3 start, Vector3 tangent, Vector3 end, int points)
        {
            Vector3[] res = new Vector3[points];
            float step = 1 / (float) (points - 1);
            for (int i = 0; i < points; i++)
            {
                res[i] = GetBezierPoint(start, tangent, end, step * i);
            }
            return res;
        }
        
        public static float Remap (float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}