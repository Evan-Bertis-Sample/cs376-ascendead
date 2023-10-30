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
            var colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, interactableLayer);
            DialogueRunner nearestRunner = null;
            float nearestDistance = float.MaxValue;
            int totalInRadius = 0;
            int totalWithLineOfSight = 0;

            foreach (var collider in colliders)
            {
                var runner = collider.GetComponent<DialogueRunner>();
                if (runner != null) totalInRadius++;
                if (runner != null && HasLineOfSight(runner.transform))
                {
                    totalWithLineOfSight++;
                    float distance = Vector2.Distance(transform.position, runner.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestRunner = runner;
                    }
                }
            }
            Debug.Log("DialogueInquirer : Found " + totalInRadius + " runners in radius, " + totalWithLineOfSight + " with line of sight.");
            return nearestRunner;
        }

        private bool HasLineOfSight(Transform target)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, obstacleLayer);
            return hit.collider == null || hit.transform == target; // No obstacle or the obstacle is the target
        }
    }
}
