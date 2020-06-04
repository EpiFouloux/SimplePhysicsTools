using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePhysicsTools.Tools.Path
{
    public class PathFollowersHandler : MonoBehaviour
    {
        private PathEvaluator _evaluator;
        public PathEvaluator Evaluator
        {
            get => _evaluator;
            set
            {
                _evaluator = value;
                foreach (PathFollower follower in followers) 
                    follower.Path = value;
            }
        }

        private bool _ready;
        private bool Ready
        {
            get =>  _ready && _evaluator != null;
            set => _ready = value;
        }

        [SerializeField] private List<PathFollower> followers;
        [SerializeField] private float launchDelay = 2f;
        [SerializeField] private bool launchOnStartup = true;

        private void Awake()
        {
            foreach (PathFollower follower in followers) 
                follower.Active = false;
        }

        private void Start()
        {
            if (launchOnStartup)
                Launch();
        }

        public void Launch()
        {
            StopAllCoroutines();
            Ready = true;
            StartCoroutine(Cor_Launch());
        }

        private IEnumerator Cor_Launch()
        {
            var wait = new WaitForEndOfFrame();
            var delay = new WaitForSeconds(launchDelay);
            while (!Ready)
            {
                yield return wait;
            }
            foreach (PathFollower pathFollower in followers)
            {
                pathFollower.Active = true;
                yield return delay;
            }
        }
    }
}