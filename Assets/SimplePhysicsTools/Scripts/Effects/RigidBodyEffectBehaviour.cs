using UnityEditor;
using UnityEngine;

namespace SimplePhysicsTools.Effects
{
    public abstract class RigidBodyEffectBehaviour : EffectBehaviour
    {
        [SerializeField] protected bool ignoreParentRigidbody = true;

        protected Rigidbody parentBody;

        #region MonoBehaviour
        
        protected override void Awake()
        {
            base.Awake();
            parentBody = GetComponentInParent<Rigidbody>();
        }

        #endregion
    }
    
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RigidBodyEffectBehaviour))]
    public class RigidBodyEffectBehaviourEditor : EffectBehaviourEditor
    {
        private SerializedProperty ignoreParentRigidbody;
        protected override void Init()
        {
            base.Init();
            ignoreParentRigidbody = serializedObject.FindProperty("ignoreParentRigidbody");
        }

        private void OnEnable()
        {
            Init();
        }

        protected override void DrawActivationConfiguration()
        {
            base.DrawActivationConfiguration();
            EditorGUILayout.PropertyField(ignoreParentRigidbody);
        }
    }
#endif
}