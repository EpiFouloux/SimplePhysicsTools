using System.Collections.Generic;
using SimplePhysicsTools.Tools.Events;
using UnityEngine;
using UnityEditor;

namespace SimplePhysicsTools.Tools.Path
{
     public class PathDrawer : MonoBehaviour
    {
        [SerializeField] public List<Vector3> mainPath = new List<Vector3>(2);
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
                CircularList<Vector3> points = Points;
                List<Vector3> res = new CircularList<Vector3>();
                for (int i = 0; i < mainPath.Count; i++)
                {
                    if (!smooth || (!looping && (points.IsFirstIndex(i) || points.IsLastIndex(i))))
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
        
        private PathEvaluator evaluator;
        public PathEvaluator Evaluator => evaluator;

        private float currentEvaluation = 0f;

        private void Start()
        {
            Draw();
        }

        [ContextMenu("Draw")]
        public void Draw()
        {
            evaluator = new PathEvaluator(Path, looping);
            pathReady.Invoke(Evaluator);
            pointsCountReady.Invoke(evaluator.Points.Length);
            pointsReady.Invoke(evaluator.Points);
        }
    }
    
#if UNITY_EDITOR

    [CanEditMultipleObjects]
    [CustomEditor(typeof(PathDrawer))]
    public class PathDrawerEditor : Editor
    {
        private SerializedProperty mainPath;
        private SerializedProperty looping;
        private SerializedProperty smooth;
        private SerializedProperty smoothDistance;
        private SerializedProperty smoothAmount;
        private SerializedProperty pathReady;
        private SerializedProperty pointsCountReady;
        private SerializedProperty pointsReady;
        private PathDrawer script;
        private bool eventfolded;

        private void OnEnable()
        {
            script = target as PathDrawer;
            mainPath = serializedObject.FindProperty("mainPath");
            looping = serializedObject.FindProperty("looping");
            smooth = serializedObject.FindProperty("smooth");
            smoothDistance = serializedObject.FindProperty("smoothDistance");
            smoothAmount = serializedObject.FindProperty("smoothAmount");
            pathReady = serializedObject.FindProperty("pathReady");
            pointsCountReady = serializedObject.FindProperty("pointsCountReady");
            pointsReady = serializedObject.FindProperty("pointsReady");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(mainPath, true);
            if (script.mainPath.Count == 0)
            {
                script.mainPath.Add(Vector3.left);
                script.mainPath.Add(Vector3.right);
                EditorUtility.SetDirty(target);
            }
            if (script.mainPath.Count < 2)
                EditorGUILayout.HelpBox("A path can't work without at least two points", MessageType.Error);
            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(target, "Changed MovementPath");
                if (script.mainPath.Count > 0)
                    script.mainPath.Add(script.mainPath[script.mainPath.Count - 1]);
                else
                    script.mainPath.Add(Vector3.up);
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.PropertyField(looping);
            EditorGUILayout.PropertyField(smooth);
            if (smooth.boolValue)
            {
                EditorGUILayout.PropertyField(smoothAmount);
                EditorGUILayout.PropertyField(smoothDistance);
            }

            eventfolded = EditorGUILayout.Foldout(eventfolded, "Events", true, EditorStyles.foldout);
            if (eventfolded)
            {
                EditorGUILayout.PropertyField(pathReady);
                EditorGUILayout.PropertyField(pointsCountReady);
                EditorGUILayout.PropertyField(pointsReady);
            }

            EditorGUILayout.HelpBox("If you set some events to Editor and Runtime you can preview data like line renderers", MessageType.Info);
            if (GUILayout.Button("Pre Draw"))
            {
                script.Draw();
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.LabelField("PathEvaluator", script.Evaluator?.ToString());
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (!script.enabled)
                return;
            
            Handles.color = Color.magenta;
            if (script.mainPath == null || script.mainPath.Count <= 1) 
                return;
            CircularList<Vector3> path = new CircularList<Vector3>(script.mainPath);

            Quaternion handleRotation = UnityEditor.Tools.pivotRotation == PivotRotation.Local ?
                script.transform.rotation : Quaternion.identity;
            
            List<Vector3> tmpPath = new List<Vector3>(script.Path);
            if (script.Looping)
                tmpPath.Add(tmpPath[0]);
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