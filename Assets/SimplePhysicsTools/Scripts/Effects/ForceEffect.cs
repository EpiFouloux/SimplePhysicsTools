using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimplePhysicsTools.Effects
{
    /// <summary>
    /// This effect can apply forces to rigidbody colliders
    /// </summary>
    public class ForceEffect : EffectBehaviour
    {
        // Global Force Settings
        [SerializeField] protected ForceMode forceMode = ForceMode.Impulse;
        [SerializeField] protected bool ignoreParentRigidbody = true;

        // Directional Force
        [FormerlySerializedAs("knockBackForce")]
        [SerializeField] protected float directionalForceIntensity = 1f;
        [SerializeField] protected bool useDirectionalForce;
        [FormerlySerializedAs("knockBackDirection")] [SerializeField] protected Vector3 directionalForceDirection = Vector3.up;
        [SerializeField] protected bool localDirection = true;

        // Differential Directional Force
        [SerializeField] protected bool useDifferentialDirection;
        [SerializeField] protected bool inverseDirection = false;
        [SerializeField] protected float differentialForceIntensity = 1f;
        
        // Rotary Force
        [SerializeField] protected bool useRotaryDirection;
        [SerializeField] protected bool useLocalAxis;
        [Range(-90, 90)]
        [SerializeField] protected sbyte rotationAngle;
        [SerializeField] protected float rotaryForceIntensity = 1f;
        
        // Torque
        [SerializeField] protected bool useTorque;
        [SerializeField] protected bool relativeTorque;
        [SerializeField] protected Vector3 torqueForce;

        // Gizmos
        [SerializeField] public bool drawGizmos = true;
        [SerializeField] public Color gizmosColor = Color.yellow;

        public bool UseDirectionalForce => useDirectionalForce;
        public bool UseDifferentialDirection => useDifferentialDirection;
        public bool UseRotaryDirection => useRotaryDirection;

        public Vector3 DirectionalForceDirection => directionalForceDirection;
        public float DirectionalForceIntensity => directionalForceIntensity;
        public bool UseLocalDirection => localDirection;

        public sbyte RotationAngle => rotationAngle;

        public bool UseLocalAxis => useLocalAxis;

        protected Vector3 directionalForce;
        protected Rigidbody parentBody;
        protected float angleCosinus;
        protected float angleSinus;

        private Vector3 DirectionalForce =>
            directionalForceIntensity * (localDirection ? _transform.TransformDirection(directionalForceDirection) : directionalForceDirection)
            .normalized;

        private Vector3 GetDifferentialForce(Vector3 targetObjectPosition)
        {
            Vector3 position = _transform.position;
            Vector3 heading = inverseDirection ? position - targetObjectPosition : targetObjectPosition - position;
            return differentialForceIntensity * (heading / heading.magnitude);
        }

        private Vector3 GetRotaryForce(Vector3 targetObjectPosition)
        {
            Vector3 point = useLocalAxis
                ? _transform.InverseTransformPoint(targetObjectPosition)
                : targetObjectPosition;
            Vector3 relativePoint = useLocalAxis ? Vector3.zero : _transform.position;
            Vector3 targetPoint = MathTools.GetYRotatedPoint(targetObjectPosition, relativePoint
                , angleCosinus, angleSinus);
            return rotaryForceIntensity * (targetPoint - point);
        }

        #region MonoBehaviour
        
        protected override void Awake()
        {
            base.Awake();
            parentBody = GetComponentInParent<Rigidbody>();
        }

        #endregion
        
        protected override void GetReady()
        {
            base.GetReady();
            directionalForce = DirectionalForce;
            float angle = Mathf.Deg2Rad * rotationAngle;
            angleCosinus = Mathf.Cos(angle);
            angleSinus = Mathf.Sin(angle);
        }

        protected override void ApplyEffect(Collider targetCollider)
        {
            if (!overrideIgnoredColliders && ignoredColliders.Contains(targetCollider))
                return;
            Vector3 force = Vector3.zero;

            if (UseDirectionalForce)
                force += directionalForce;
            if (UseDifferentialDirection)
                force += GetDifferentialForce(targetCollider.transform.position);
            if (UseRotaryDirection)
                force += GetRotaryForce(targetCollider.transform.position);
            Rigidbody targetBody = targetCollider.GetComponentInParent<Rigidbody>();
            if (!targetBody || (ignoreParentRigidbody && targetBody == parentBody))
            {
                //   Debug.Log(gameObject.name + " ignored this non knockbackable object: " + targetCollider.name);
                return;
            }
            targetBody.AddForce(force, forceMode);
            if (useTorque)
            {
                if (relativeTorque)
                    targetBody.AddRelativeTorque(torqueForce, forceMode);
                else
                    targetBody.AddTorque(torqueForce, forceMode);
            }
        }
    }


#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ForceEffect))]
    public class ForceEffectEditor : EffectBehaviourEditor
    {
        private SerializedProperty directionalForceIntensity;
        private SerializedProperty differentialForceIntensity;
        private SerializedProperty rotaryForceIntensity;
        private SerializedProperty forceMode;
        private SerializedProperty directionalForceDirection;
        private SerializedProperty drawGizmos;
        private SerializedProperty gizmosColor;
        private SerializedProperty localDirection;
        private SerializedProperty ignoreParentRigidbody;
        private SerializedProperty useDirectionalForce;
        private SerializedProperty useDifferentialDirection;
        private SerializedProperty useRotaryDirection;
        private SerializedProperty rotationAngle;
        private SerializedProperty useLocalAxis;
        private SerializedProperty inverseDirection;
        private SerializedProperty useTorque;
        private SerializedProperty relativeTorque;
        private SerializedProperty torqueForce;
        
        private bool folded;
        private ForceEffect script;
        protected GUIStyle textStyle;

        protected override void Init()
        {
            base.Init();
            forceMode = serializedObject.FindProperty("forceMode");
            directionalForceDirection = serializedObject.FindProperty("directionalForceDirection");
            drawGizmos = serializedObject.FindProperty("drawGizmos");
            gizmosColor = serializedObject.FindProperty("gizmosColor");
            ignoreParentRigidbody = serializedObject.FindProperty("ignoreParentRigidbody");
            localDirection = serializedObject.FindProperty("localDirection");
            useDirectionalForce = serializedObject.FindProperty("useDirectionalForce");
            useDifferentialDirection = serializedObject.FindProperty("useDifferentialDirection");
            useRotaryDirection = serializedObject.FindProperty("useRotaryDirection");
            rotationAngle = serializedObject.FindProperty("rotationAngle");
            useLocalAxis = serializedObject.FindProperty("useLocalAxis");
            inverseDirection = serializedObject.FindProperty("inverseDirection");
            directionalForceIntensity = serializedObject.FindProperty("directionalForceIntensity");
            differentialForceIntensity = serializedObject.FindProperty("differentialForceIntensity");
            rotaryForceIntensity = serializedObject.FindProperty("rotaryForceIntensity");
            useTorque = serializedObject.FindProperty("useTorque");
            relativeTorque = serializedObject.FindProperty("relativeTorque");
            torqueForce = serializedObject.FindProperty("torqueForce");
        }

        private void OnEnable()
        {
            Init();
            script = target as ForceEffect;
            textStyle = new GUIStyle();
            textStyle.normal.textColor = script.gizmosColor;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            folded = EditorGUILayout.Foldout(folded, "Gizmos Configuration");
            if (folded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(drawGizmos);
                EditorGUILayout.PropertyField(gizmosColor);
                EditorGUI.indentLevel--;
            }
            DrawActivationConfiguration();
            EditorGUILayout.PropertyField(ignoreParentRigidbody);
            if (hasDetectionArea && !applyOnce.boolValue && useFixedTimeStep.boolValue)
                EditorGUILayout.HelpBox("You should probably not apply forces on every frame", MessageType.Warning);
            EditorGUILayout.HelpBox(
                "Acceleration and velocity change will ignore the mass, while force and impulse will take it in account",
                MessageType.Info);
            EditorGUILayout.PropertyField(forceMode);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Forces", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(useDifferentialDirection);
            if (useDifferentialDirection.boolValue)
            {
                EditorGUILayout.HelpBox(
                    inverseDirection.boolValue
                        ? "The objects will be attracted to the object pivot"
                        : "The objects will be pushed away from the object pivot", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(inverseDirection);
                EditorGUILayout.PropertyField(differentialForceIntensity);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(useDirectionalForce);
            if (useDirectionalForce.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(directionalForceIntensity);
                EditorGUILayout.PropertyField(directionalForceDirection);
                EditorGUILayout.PropertyField(localDirection);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(useRotaryDirection);
            if (useRotaryDirection.boolValue)
            {
                EditorGUILayout.HelpBox("The rotation axis will always be the Y axis, world or local", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(rotaryForceIntensity);
                EditorGUILayout.PropertyField(rotationAngle);
                EditorGUILayout.PropertyField(useLocalAxis);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(useTorque);
            if (useTorque.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(relativeTorque);
                EditorGUILayout.PropertyField(torqueForce);
                EditorGUI.indentLevel--;
            }

            DrawEventsConfiguration();
            DrawDebug();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (!script.drawGizmos || !script.enabled)
                return;
            Handles.color = script.gizmosColor;
            if (script.UseDirectionalForce && script.DirectionalForceDirection != Vector3.zero && script.DirectionalForceIntensity > 0)
            {
                Vector3 arrowCenter;
                Vector3 labelPosition = script.DirectionalForceDirection.normalized * (script.DirectionalForceIntensity / 2);
                if (script.UseLocalDirection)
                {
                    Handles.matrix = script.transform.localToWorldMatrix;
                    arrowCenter = Vector3.zero;
                }
                else
                {
                    arrowCenter = script.transform.position;
                    Handles.matrix = Matrix4x4.identity;
                }

                Handles.ArrowHandleCap(0, arrowCenter, Quaternion.LookRotation(script.DirectionalForceDirection),
                    script.DirectionalForceIntensity, EventType.Repaint);
                Handles.Label(arrowCenter + labelPosition, "KnockBack Force Vector", textStyle);
            }

            if (script.UseRotaryDirection)
            {
                Vector3 center = Vector3.zero;
                Vector3 normal = Vector3.up;
                if (script.UseLocalAxis)
                {
                    Handles.matrix = script.transform.localToWorldMatrix;
                }
                else
                {
                    center = script.transform.position;
                    Handles.matrix = Matrix4x4.identity;
                }
                Handles.DrawSolidArc(center, normal, Vector3.back, script.RotationAngle, 2f);
            }
        }
    }
    
#endif
}