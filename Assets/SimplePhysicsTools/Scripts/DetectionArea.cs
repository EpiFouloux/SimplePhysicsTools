using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SimplePhysicsTools
{
    /// <summary>
    /// Type of detection Area, will define GUI area and physical overlap area
    /// </summary>
    public enum AreaType
    {
        Sphere,
        Box,
        Capsule,
    }

    /// <summary>
    /// Structure used to contain detected colliders from a DetectionArea.
    /// targetCount gives the number of detected elements contained in a fixed size array.
    /// </summary>
    public struct AreaTargets
    {
        public Collider[] targetColliders;
        public uint targetCount;
    }
    
    /// <summary>
    /// This component allows to retrieve colliders through Physic overlap with a powerful GUI customization.
    /// </summary>
    [DisallowMultipleComponent]
    public class DetectionArea : MonoBehaviour
    {
        private static readonly Dictionary<CapsuleBoundsHandle.HeightAxis, Vector3> CapsuleAxis =
            new Dictionary<CapsuleBoundsHandle.HeightAxis, Vector3>()
            {
                { CapsuleBoundsHandle.HeightAxis.X, Vector3.right},
                { CapsuleBoundsHandle.HeightAxis.Y, Vector3.up},
                { CapsuleBoundsHandle.HeightAxis.Z, Vector3.forward }
            };
        
        private Transform _transform;
        [SerializeField] public AreaType areaType;
        [SerializeField] public float sphereAreaRadius = 1;
        [SerializeField] public float capsuleAreaRadius = 1;
        [SerializeField] public float capsuleHeight = 1;
        [SerializeField] public Vector3 capsuleCenter;
        [SerializeField] public Vector3 sphereCenter;
        [SerializeField] public CapsuleBoundsHandle.HeightAxis capsuleHeightAxis;
        [SerializeField] public Bounds boxAreaBounds;
        [SerializeField] public Color gizmosColor = Color.green;
        [SerializeField] public bool showGizmos = true;
        [SerializeField] public bool lockCenter = true;
        [SerializeField] public LayerMask areaMask;
        [SerializeField] private byte maxDetectedItems = 15;
        [SerializeField] public string areaName = "Detection Area";
     
        public string AreaName => areaName;
        private Collider[] detectedColliders;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            detectedColliders = new Collider[maxDetectedItems];
            if (areaMask.value == 0)
                Debug.LogError("Please set up a layer mask for a working detection area.");
        }

        /// <summary>
        /// Will retrieve the colliders in the customized Area.
        /// Call this getter in physics frame (FixedUpdate)
        /// </summary>
        public AreaTargets AreaTargets
        {
            get
            {
                AreaTargets res = new AreaTargets();
                switch (areaType)
                {
                    case AreaType.Box:
                        res.targetCount = (uint) Physics.OverlapBoxNonAlloc(
                            _transform.TransformPoint(boxAreaBounds.center),boxAreaBounds.extents,
                            detectedColliders, transform.rotation, areaMask, QueryTriggerInteraction.Ignore);

                        break;
                    case AreaType.Sphere:
                        res.targetCount = (uint) Physics.OverlapSphereNonAlloc(
                            _transform.TransformPoint(sphereCenter), sphereAreaRadius, detectedColliders, areaMask);
                        break;
                    case AreaType.Capsule:
                        Vector3 deltaPoint = (capsuleHeight / 2 - capsuleAreaRadius) * CapsuleAxis[capsuleHeightAxis];
                        Vector3 startpoint = _transform.TransformPoint(capsuleCenter - deltaPoint);
                        Vector3 endPoint = _transform.TransformPoint(capsuleCenter + deltaPoint);
                        
                        res.targetCount = (uint) Physics.OverlapCapsuleNonAlloc(startpoint, endPoint,
                            capsuleAreaRadius, detectedColliders, areaMask);
                        break;
                }
                if (res.targetCount >= maxDetectedItems)
                    Debug.LogWarning("Maximum of " + maxDetectedItems + " detected objects reached on " + name);
                res.targetColliders = detectedColliders;
                return res;
            }
        }
    }
    
#if UNITY_EDITOR

    [CanEditMultipleObjects]
    [CustomEditor(typeof(DetectionArea))]
    public class DetectionAreaEditor : Editor
    {
        protected SerializedProperty areaType;
        protected SerializedProperty sphereAreaRadius;
        protected SerializedProperty boxAreaBounds;
        protected SerializedProperty gizmosColor;
        protected SerializedProperty showGizmos;
        protected SerializedProperty areaMask;
        protected SerializedProperty maxDetectedItems;
        protected SerializedProperty areaName;
        protected SerializedProperty capsuleAreaRadius;
        protected SerializedProperty capsuleHeight;
        protected SerializedProperty capsuleHeightAxis;
        protected SerializedProperty capsuleCenter;
        protected SerializedProperty sphereCenter;
        protected SerializedProperty lockCenter;
        
        protected GUIStyle textStyle;
        protected DetectionArea script;
        protected BoxBoundsHandle boxBoundsHandle = new BoxBoundsHandle();
        protected SphereBoundsHandle sphereBoundsHandle = new SphereBoundsHandle();
        protected CapsuleBoundsHandle capsuleBoundsHandle = new CapsuleBoundsHandle();

        protected virtual void Init()
        {
            areaType = serializedObject.FindProperty("areaType");
            sphereAreaRadius = serializedObject.FindProperty("sphereAreaRadius");
            boxAreaBounds = serializedObject.FindProperty("boxAreaBounds");
            gizmosColor = serializedObject.FindProperty("gizmosColor");
            showGizmos = serializedObject.FindProperty("showGizmos");
            areaMask = serializedObject.FindProperty("areaMask");
            maxDetectedItems = serializedObject.FindProperty("maxDetectedItems");
            areaName = serializedObject.FindProperty("areaName");
            capsuleAreaRadius = serializedObject.FindProperty("capsuleAreaRadius");
            capsuleHeight = serializedObject.FindProperty("capsuleHeight");
            capsuleHeightAxis = serializedObject.FindProperty("capsuleHeightAxis");
            capsuleCenter = serializedObject.FindProperty("capsuleCenter");
            sphereCenter = serializedObject.FindProperty("sphereCenter");
            lockCenter = serializedObject.FindProperty("lockCenter");
        }
        
        private void OnEnable()
        {
            Init();
            script = (DetectionArea) target;
            textStyle = new GUIStyle();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if (script.transform.lossyScale != Vector3.one ||script.transform.localScale != Vector3.one)
                EditorGUILayout.HelpBox("The detection area script does not handle scaling, " +
                                        "visual area and actual physical zone won't match", MessageType.Error);
            EditorGUILayout.LabelField("GUI config", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showGizmos);
            EditorGUILayout.PropertyField(gizmosColor);
            EditorGUILayout.PropertyField(areaName);
            EditorGUILayout.LabelField("Physics config", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(areaMask);
            if (areaMask.intValue == 0)
                EditorGUILayout.HelpBox("You need to set a valid layer mask", MessageType.Error);
            EditorGUILayout.PropertyField(maxDetectedItems);
            EditorGUILayout.HelpBox("The maximum amount of detected items per frame can influence" +
                                    " performance highly", MessageType.Info);
            EditorGUILayout.LabelField("Shape config", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(areaType);
            switch (script.areaType)
            {
                case AreaType.Box:
                    EditorGUILayout.PropertyField(boxAreaBounds);
                    break;
                case AreaType.Sphere:
                    EditorGUILayout.PropertyField(sphereAreaRadius);
                    EditorGUILayout.PropertyField(sphereCenter);
                    break;
                case AreaType.Capsule:
                    EditorGUILayout.PropertyField(capsuleAreaRadius);
                    EditorGUILayout.PropertyField(capsuleHeight);
                    EditorGUILayout.PropertyField(capsuleHeightAxis);
                    EditorGUILayout.PropertyField(capsuleCenter);
                    break;
            }
            EditorGUILayout.PropertyField(lockCenter);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (!script.enabled || !script.showGizmos)
                return;

            Handles.matrix = script.transform.localToWorldMatrix;
            Handles.color = script.gizmosColor;
            textStyle.normal.textColor = script.gizmosColor;

            boxBoundsHandle.size = script.boxAreaBounds.size;
            sphereBoundsHandle.radius = script.sphereAreaRadius;
            capsuleBoundsHandle.radius = script.capsuleAreaRadius;
            capsuleBoundsHandle.height = script.capsuleHeight;
            capsuleBoundsHandle.heightAxis = script.capsuleHeightAxis;

            EditorGUI.BeginChangeCheck();
            
            switch (script.areaType)
            {
                case AreaType.Box:
                    Handles.Label(boxBoundsHandle.center + boxBoundsHandle.size / 2, script.AreaName, textStyle);
                    if (!script.lockCenter)
                        boxBoundsHandle.center = Handles.PositionHandle(script.boxAreaBounds.center, Quaternion.identity);
                    else
                        boxBoundsHandle.center = script.boxAreaBounds.center; 
                    boxBoundsHandle.DrawHandle();
                    break;
                case AreaType.Sphere:
                    Handles.Label(Vector3.up * script.sphereAreaRadius, script.AreaName, textStyle);
                    if (!script.lockCenter)
                        sphereBoundsHandle.center = Handles.PositionHandle(script.sphereCenter, Quaternion.identity);
                    else
                        sphereBoundsHandle.center = script.sphereCenter; 
                    sphereBoundsHandle.DrawHandle();
                    break;
                case AreaType.Capsule:
                    Handles.Label(capsuleBoundsHandle.center, script.AreaName, textStyle);
                    if (!script.lockCenter)
                        capsuleBoundsHandle.center = Handles.PositionHandle(script.capsuleCenter, Quaternion.identity);
                    else
                        capsuleBoundsHandle.center = script.capsuleCenter; 
                    capsuleBoundsHandle.DrawHandle();
                    break;
            }
        
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Detection Area");
                Bounds newBounds = new Bounds();
                if (!script.lockCenter)
                {
                    script.sphereCenter = sphereBoundsHandle.center;
                    script.capsuleCenter = capsuleBoundsHandle.center;
                    newBounds.center = boxBoundsHandle.center;
                }
                else
                    newBounds.center = script.boxAreaBounds.center;
                newBounds.size = boxBoundsHandle.size;
                script.boxAreaBounds = newBounds;
                script.sphereAreaRadius = sphereBoundsHandle.radius;
                script.capsuleHeight = capsuleBoundsHandle.height;
                script.capsuleAreaRadius = capsuleBoundsHandle.radius;
                script.capsuleHeightAxis = capsuleBoundsHandle.heightAxis;
                EditorUtility.SetDirty(target);
            }
        }
    }
    
#endif
}