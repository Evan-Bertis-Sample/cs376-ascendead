using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurlyCore;
using CurlyCore.Input;
using UnityEngine;
using static CurlyUtility.TaskUtility;

namespace Ascendead.Dialogue
{
    public class DialogueInquirer : MonoBehaviour
    {
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField, InputPath] private string _interactPrompt;

        [GlobalDefault] private InputManager _inputManager;
        [SerializeField] private bool _isRunningDialogue = false;

        private List<DialogueRunner> _runnersInRange = new List<DialogueRunner>();

        private void Awake()
        {
            DependencyInjector.InjectDependencies(this);
            _isRunningDialogue = false;
        }

        private void Update()
        {
            if (_isRunningDialogue) return;


            if (_inputManager.GetInputDown(_interactPrompt))
            {
                Debug.Log("Requesting dialogue");

                var nearestRunner = FindNearestRunnerWithLineOfSight();
                if (nearestRunner != null)
                {
                    Debug.Log("Found runner, running dialogue");
                    StartCoroutine(RunDialogueCoroutine(nearestRunner));
                }
            }
        }

        public void RegisterRunner(DialogueRunner runner)
        {
            _runnersInRange.Add(runner);
        }

        public void UnregisterRunner(DialogueRunner runner)
        {
            _runnersInRange.Remove(runner);
        }

        private IEnumerator RunDialogueCoroutine(DialogueRunner runner)
        {
            Debug.Log("Running dialogue coroutine");
            _isRunningDialogue = true;
            yield return runner.RunDialogueCoroutine();
            _isRunningDialogue = false;
            Debug.Log("Dialogue coroutine finished");
        }

        private DialogueRunner FindNearestRunnerWithLineOfSight()
        {
            if (_runnersInRange.Count == 0) return null;
            
            DialogueRunner nearestRunner = null;
            float nearestDistance = float.MaxValue;
            foreach (var runner in _runnersInRange)
            {
                if (runner == null) continue;
                if (!HasLineOfSight(runner.transform)) continue;

                float distance = Vector2.Distance(transform.position, runner.transform.position);
                if (distance < nearestDistance)
                {
                    nearestRunner = runner;
                    nearestDistance = distance;
                }
            }
            return nearestRunner;
        }

        private bool HasLineOfSight(Transform target)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, obstacleLayer);
            return hit.collider == null || hit.transform == target; // No obstacle or the obstacle is the target
        }
    }
}
