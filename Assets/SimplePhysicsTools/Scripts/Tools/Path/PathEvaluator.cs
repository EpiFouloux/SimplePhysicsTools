using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimplePhysicsTools.Tools.Path
{
    [Serializable]
    public class PathEvaluator
    {
        public readonly List<Vector3> points;
        private Dictionary<float, Vector3> pointDistances;
        private float totalDistance = 0f;
        private bool loopingPath;

        public Vector3[] Points => points.ToArray();
        public Vector3 StartPoint => points.First();
        public Vector3 EndPoint => points.Last();

        public float TotalDistance => totalDistance;

        public PathEvaluator(List<Vector3> points, bool loopingPath = false)
        {
            this.points = new List<Vector3>(points.Count);
            foreach (var point in points)
            {
                if (!this.points.Contains(point))
                    this.points.Add(point);
            }
            this.loopingPath = loopingPath;
            HandleDistances();
        }
        
        public PathEvaluator(PathEvaluator duplicatedCurve, Transform relativeTransform, bool loopingPath = false)
        { 
            this.loopingPath = loopingPath;
            Vector3[] dupPoints = duplicatedCurve.Points;
            points = new List<Vector3>(dupPoints.Length);
            for (int i = 0; i < dupPoints.Length; i++)
            {
                points.Add(relativeTransform.TransformPoint(dupPoints[i]));
            }
            HandleDistances();
        }

        public override string ToString()
        {
            return "{" + totalDistance.ToString("F") + ", " + points?.Count + ", " + loopingPath + "}";
        }

        private void HandleDistances()
        {
            pointDistances = new Dictionary<float, Vector3>(points.Count);
            Vector3 previousPoint = points.First();
            
            foreach (Vector3 point in points)
            {
                totalDistance += Vector3.Distance(point, previousPoint);
                pointDistances.Add(totalDistance, point);
                previousPoint = point;
            }

            if (loopingPath)
            {
                totalDistance += Vector3.Distance(points.First(), points.Last());
                pointDistances.Add(totalDistance, points.First());
            }
        }   
        
        public Vector3 Evaluate(float time)
        {
            time = Mathf.Clamp01(time);
            float ratio = 1f / (points.Count - 1);

            float place = time / ratio;
            int index1 = Mathf.RoundToInt(place);
            int index2;
            if (index1 > place)
            {
                index2 = index1;
                index1--;
            }
            else
                index2 = index1 + 1;
            float lerpValue = place - index1;
            return Vector3.Lerp(points[index1], points[index2], lerpValue);
        }

        public bool NextPosition(ref Vector3 nextPosition, ref float currentDistance, float newDistance)
        {
            Vector3 direction = new Vector3();
            return NextPosition(ref nextPosition, ref direction, ref currentDistance, newDistance);
        }

        public bool NextPosition(ref Vector3 nextPosition, ref Vector3 direction, ref float currentDistance, float newDistance)
        {
            float oldDistance = currentDistance;
            float targetDistance = oldDistance + newDistance;
            
            KeyValuePair<float, Vector3> previousPos = pointDistances.Last(x => x.Key <= oldDistance);
            float previousDelta = oldDistance - previousPos.Key;
            if (targetDistance >= totalDistance)
            {
                if (loopingPath)
                {
                    nextPosition = pointDistances[0f];
                    currentDistance = 0f;
                    return true;
                }
                nextPosition = pointDistances[totalDistance];
                return false;
            }
            Vector3 nextPos = pointDistances.First(x => x.Key >= targetDistance).Value;
            direction = nextPos - previousPos.Value;
            currentDistance += newDistance;
            nextPosition = Vector3.MoveTowards(previousPos.Value, nextPos, previousDelta + newDistance);
            return true;
        }
    }
}