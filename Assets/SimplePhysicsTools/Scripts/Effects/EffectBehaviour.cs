using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SimplePhysicsTools.Effects
{
    /// <summary>
    /// Abstract class to apply effect on elements, can be used with <see cref="DetectionArea"/> component or with a collider.
    /// Can be launched on start up or launched through script with the ability to define a caster collider that we can ignore
    /// </summary>
    public abstract class EffectBehaviour : MonoBehaviour
    {
        [SerializeField] protected bool applyEffectOnCollision = false;
        [SerializeField] protected bool overrideIgnoredColliders = false;
        [SerializeField] protected bool useFixedTimeStep = true;
        [SerializeField] protected float customTimeStep = .1f;
        [SerializeField] protected bool launchOnStartUp;
        [SerializeField] protected bool isLaunched;
        [SerializeField] protected bool applyOnce;

        [SerializeField] private float timeStepCountDown;

        [SerializeField] public UnityEvent onEffectStart;
        [SerializeField] public UnityEvent onEffectStop;
        
        public bool IsLaunched => isLaunched;
        public bool UseFixedTimeStep => useFixedTimeStep;

        protected Transform _transform;
        protected DetectionArea detectionArea;
        protected List<Collider> ignoredColliders;

        #region Monobehaviour

        protected virtual void Awake()
        {
            ignoredColliders = new List<Collider>();
            _transform = GetComponent<Transform>();
            detectionArea = GetComponent<DetectionArea>();
        }
        
        private void Start()
        {
            if (launchOnStartUp)
                Launch();
        }
        
        private void FixedUpdate()
        {
            if (!isLaunched || detectionArea == null)
                return;
            if (useFixedTimeStep)
                ApplyEffectOnTargets();
            else
            {
                if (timeStepCountDown <= 0f)
                {
                    ApplyEffectOnTargets();
                    timeStepCountDown = customTimeStep;
                }
                else
                {
                    timeStepCountDown -= Time.fixedDeltaTime;
                }
            }
        }

        #endregion

        private void ApplyEffectOnTargets()
        {
            if (detectionArea == null)
                throw new NotImplementedException("Effect Behaviours need DetectionArea components");
            GetReady();
            AreaTargets targets = detectionArea.AreaTargets;
            for (int i = 0; i < targets.targetCount; i++) 
                ApplyEffect(targets.targetColliders[i]);
        }
        
        /// <summary>
        /// Launches the effect
        /// </summary>
        /// <param name="collidersToIgnore">specified colliders will ignored by effects, can be overriden</param>
        public void Launch(ICollection<Collider> collidersToIgnore = null)
        {
            timeStepCountDown = 0f;
            if (applyOnce)
                ApplyEffectOnTargets();
            else
                isLaunched = true;
            if (collidersToIgnore != null)
                ignoredColliders.AddRange(collidersToIgnore);
            onEffectStart.Invoke();
        }
        
        /// <summary>
        /// Stops the running effect
        /// </summary>
        public void Stop()
        {
            isLaunched = false;
            ignoredColliders.Clear();
            onEffectStop.Invoke();
        }

        /// <summary>
        /// If the effect is started, it will stop it, otherwise it starts it
        /// </summary>
        public void Toggle()
        {
            if (isLaunched)
                Stop();
            else
                Launch();
        }

        protected virtual void GetReady() {}

        protected abstract void ApplyEffect(Collider collider);

        #region Collision

        private void OnCollisionEnter(Collision other)
        {
            GetReady();
            if (IsLaunched && applyEffectOnCollision)
            {
                ApplyEffect(other.collider);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            GetReady();
            if (IsLaunched && applyEffectOnCollision)
            {
                ApplyEffect(other);
            }
        }

        #endregion
     
    }
    
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(EffectBehaviour), true)]
    public class EffectBehaviourEditor : Editor
    {
        protected SerializedProperty applyEffectOnCollision;
        protected SerializedProperty overrideIgnoredColliders;
        protected SerializedProperty useFixedTimeStep;
        protected SerializedProperty customTimeStep;
        protected SerializedProperty launchOnStartUp;
        protected SerializedProperty timeStepCountDown;
        protected SerializedProperty onEffectStart;
        protected SerializedProperty onEffectStop;
        protected SerializedProperty applyOnce;
        private bool eventsFolded = false;
        private EffectBehaviour script;
        protected bool hasDetectionArea;
        
        protected virtual void Init()
        {
            applyEffectOnCollision = serializedObject.FindProperty("applyEffectOnCollision");
            overrideIgnoredColliders = serializedObject.FindProperty("overrideIgnoredColliders");
            useFixedTimeStep = serializedObject.FindProperty("useFixedTimeStep");
            customTimeStep = serializedObject.FindProperty("customTimeStep");
            launchOnStartUp = serializedObject.FindProperty("launchOnStartUp");
            timeStepCountDown = serializedObject.FindProperty("timeStepCountDown");
            onEffectStart = serializedObject.FindProperty("onEffectStart");
            onEffectStop = serializedObject.FindProperty("onEffectStop");
            applyOnce = serializedObject.FindProperty("applyOnce");
            script = target as EffectBehaviour;
        }

        private void OnEnable()
        {
            Init();
        }

        protected void DrawActivationConfiguration()
        {
            EditorGUILayout.LabelField("Activation Configuration", EditorStyles.boldLabel);
            hasDetectionArea = script.GetComponent<DetectionArea>();
            EditorGUILayout.PropertyField(launchOnStartUp);
            if (!hasDetectionArea)
            {
                if (!applyEffectOnCollision.boolValue)
                {
                    EditorGUILayout.HelpBox("If the effect is not applied on collision add a DetectionArea", MessageType.Error);
                    if (GUILayout.Button("Add DetectionArea Component"))
                    {
                        script.gameObject.AddComponent<DetectionArea>();
                        EditorUtility.SetDirty(script);
                    }
                }
            }
            EditorGUILayout.PropertyField(applyEffectOnCollision);
            if (applyEffectOnCollision.boolValue && !script.GetComponent<Collider>())
                EditorGUILayout.HelpBox("The object requires a collider", MessageType.Error);
            EditorGUILayout.PropertyField(overrideIgnoredColliders);
            if (!applyEffectOnCollision.boolValue && hasDetectionArea)
            {
                EditorGUILayout.PropertyField(applyOnce);
                if (!applyOnce.boolValue)
                {
                    EditorGUILayout.PropertyField(useFixedTimeStep);
                    if (!useFixedTimeStep.boolValue)
                        EditorGUILayout.PropertyField(customTimeStep);  
                }
            }
        }

        protected void DrawEventsConfiguration()
        {
            EditorGUILayout.Space();
            eventsFolded = EditorGUILayout.Foldout(eventsFolded, "Events", EditorStyles.foldoutHeader);
            if (eventsFolded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(onEffectStart);
                EditorGUILayout.PropertyField(onEffectStop);
                EditorGUI.indentLevel--;
            }
        }

        protected void DrawDebug()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Is Launched: ", script.IsLaunched.ToString());
            EditorGUILayout.LabelField("Time step Countdown", timeStepCountDown.floatValue.ToString());
        }
        
        public override void OnInspectorGUI()
        {
            DrawActivationConfiguration();
            DrawEventsConfiguration();
            DrawDebug();
        }
    }

#endif
}