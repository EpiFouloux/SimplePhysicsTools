using System.Collections.Generic;
using SimplePhysicsTools.Tools.Events;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace SimplePhysicsTools.Tools.Path
{
    public class PathDrawer : MonoBehaviour
    {
        [SerializeField] public List<Vector3> mainPath;
        [SerializeField] private bool looping;
        [SerializeField] private bool smooth;
        [SerializeField] [Range(1, 15)] private float smoothDistance = 1f;
        [SerializeField] [Range(2, 100)] private byte smoothAmount = 10;
        public PathEvaluatorEvent pathReady;
        public IntEvent pointsCountReady;
        public Vector3ArrayEvent pointsReady;
        
        public bool Looping => looping;
        public bool Smooth => smooth;
        public float SmoothDistance => smoothDistance;

        public CircularList<Vector3> Points => new CircularList<Vector3>(mainPath);
        public List<Vector3> Path
        {
            get
            {
                if (!smooth)
                    return mainPath;
                CircularList<Vector3> points = Points;
                List<Vector3> res = new CircularList<Vector3>();
                for (int i = 0; i < mainPath.Count; i++)
                {
                    if (!looping && (points.IsFirstIndex(i) || points.IsLastIndex(i)))
                    {
                        res.Add(transform.TransformPoint(points[i]));
                        continue;
                    }
                    int j = points.PreviousIndex(i);
                    float distance = Mathf.Clamp(SmoothDistance, 0f, Vector3.Distance(points[i], points[j]) / 2);
                    Vector3 previousPoint = points[i] + (points[j] - points[i]).normalized * distance;
                    j = points.NextIndex(i);
                    distance = Mathf.Clamp(SmoothDistance, 0f, Vector3.Distance(points[i], points[j]) / 2);
                    Vector3 nextPoint = points[i] + (points[j] - points[i]).normalized * distance;
                    res.AddRange(MathTools.GetBezierCurve(
                        transform.TransformPoint(previousPoint),
                        transform.TransformPoint(points[i]),
                        transform.TransformPoint(nextPoint),
                        smoothAmount)
                    );
                }
                return res;
            }
        }
        
        private PathEvaluator _evaluator;
        public PathEvaluator Evaluator => _evaluator;

        private float currentEvaluation = 0f;

        private void Start()
        {
            _evaluator = new PathEvaluator(Path, looping);
            pathReady.Invoke(Evaluator);
            pointsCountReady.Invoke(_evaluator.Points.Length);
            pointsReady.Invoke(_evaluator.Points);
        }
    }
    
#if UNITY_EDITOR

    [CanEditMultipleObjects]
    [CustomEditor(typeof(PathDrawer))]
    public class PathDrawerEditor : Editor
    {private PathDrawer script;

        private void OnEnable()
        {
            script = target as PathDrawer;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if (script.mainPath.Count == 0)
            {
                script.mainPath.Add(Vector3.up);
                EditorUtility.SetDirty(target);
            }
            DrawDefaultInspector();
            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(target, "Changed MovementPath");
                if (script.mainPath.Count > 0)
                    script.mainPath.Add(script.mainPath[script.mainPath.Count - 1]);
                else
                    script.mainPath.Add(Vector3.up);
                EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (!script.enabled)
                return;
            
            Handles.color = Color.magenta;
            CircularList<Vector3> path = new CircularList<Vector3>(script.mainPath);
            if (path == null || path.Count <= 0) 
                return;

            Quaternion handleRotation = UnityEditor.Tools.pivotRotation == PivotRotation.Local ?
                script.transform.rotation : Quaternion.identity;
            
            List<Vector3> tmpPath = new List<Vector3>(script.Path);
            if (script.Looping)
                tmpPath.Add(tmpPath[0]);
            if (script.Smooth)
                Handles.DrawPolyLine(tmpPath.ToArray());
            else
                Handles.DrawPolyLine(tmpPath.ToArray());

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < path.Count; i++)
            {
                path[i] = Handles.PositionHandle(script.transform.TransformPoint(path[i]), handleRotation);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed MovementPath");
                for (int i = 0; i < path.Count; i++)
                {
                    script.mainPath[i] = script.transform.InverseTransformPoint(path[i]);
                }
                EditorUtility.SetDirty(target);
            }
        }
    }
    
#endif
}