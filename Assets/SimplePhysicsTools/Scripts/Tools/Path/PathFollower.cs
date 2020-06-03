using UnityEngine;

namespace SimplePhysicsTools.Tools.Path
{ 
    public enum PathFollowMode
    {
        Evaluation,
        DistanceBased
    }
    
    [RequireComponent(typeof(Rigidbody))]
    public class PathFollower : MonoBehaviour
    {
        private Rigidbody _body;
        private PathEvaluator _path;
        
        [SerializeField] private bool active = false;
        [SerializeField] private float speed = 10;
        [SerializeField] private Vector3 offset = Vector3.up;
        [SerializeField] private PathFollowMode followMode;
        [SerializeField] private bool faceDirection;

        private float _currentDistance = 0f;
        private Vector3 _facingDirection;
        private Vector3 _nextPosition;

        public PathEvaluator Path
        {
            get => _path;
            set => _path = value;
        }

        public bool Active
        {
            get => active && _path != null;
            set => active = value;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _facingDirection = transform.forward;
        }

        private void FixedUpdate()
        {
            if (!Active)
                return;
            bool running = _path.NextPosition(ref _nextPosition, ref _facingDirection, ref _currentDistance,
                speed * Time.fixedDeltaTime);
            _body.MovePosition(_nextPosition + offset);
            if (faceDirection)
                _body.MoveRotation(Quaternion.LookRotation(_facingDirection));
            if (!running)
                Active = false;
        }
    }
}