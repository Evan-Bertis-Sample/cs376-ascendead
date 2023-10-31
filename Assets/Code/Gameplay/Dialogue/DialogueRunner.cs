using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurlyUtility;
using UnityEngine;

namespace Ascendead.Dialogue
{
    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField, FilePath] private string _dialoguePath;
        [SerializeField] private string _characterName; // Character's name field
        [SerializeField] private float _detectionRadius = 5f;
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private GameObject _displayPrompt;
        [SerializeField] private Vector2 _displayPromptOffset;

        private DialogueTree _dialogueTree;
        private DialogueNode _currentNode;

        public DialogueFrontendObject DialogueFrontend;
        private List<DialogueInquirer> _regsteredInquirers = new List<DialogueInquirer>();
        private GameObject _displayPromptInstance;

        public delegate void DialogueEventHandler(string eventName, object[] parameters);
        public event DialogueEventHandler OnDialogueEvent;

        void Start()
        {
            _dialoguePath = "";
            string path = _dialoguePath;
            SetDialogueTree(path);

            // spawn display prompt
            _displayPromptInstance = Instantiate(_displayPrompt, transform.position + (Vector3)_displayPromptOffset, Quaternion.identity);
            _displayPromptInstance.transform.SetParent(transform);
        }

        private void Update()
        {
            // Make sure to remove yourself from nearby DialogueInquirers

            List<DialogueInquirer> toRemove = new List<DialogueInquirer>();
            foreach (var inquirer in _regsteredInquirers)
            {
                if (inquirer == null) continue;
                if (!HasLineOfSight(inquirer.transform) || Vector2.Distance(transform.position, inquirer.transform.position) > _detectionRadius)
                {
                    inquirer.UnregisterRunner(this);
                    toRemove.Add(inquirer);
                }
            }

            foreach (var inquirer in toRemove)
            {
                _regsteredInquirers.Remove(inquirer);
            }

            // add yourself to nearby DialogueInquirers
            var colliders = Physics2D.OverlapCircleAll(transform.position, _detectionRadius, _interactableLayer);
            foreach (var collider in colliders)
            {
                var inquirer = collider.GetComponent<DialogueInquirer>();
                if (inquirer != null && HasLineOfSight(inquirer.transform))
                {
                    inquirer.RegisterRunner(this);
                    _regsteredInquirers.Add(inquirer);
                }
            }

            // show display prompt if there are nearby DialogueInquirers
            _displayPromptInstance.SetActive(_regsteredInquirers.Count > 0);
        }

        private bool HasLineOfSight(Transform target)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, _obstacleLayer);
            return hit.collider == null || hit.transform == target; // No obstacle or the obstacle is the target
        }

        public void SetDialogueTree(string path)
        {
            if (_dialoguePath == path) return; // If the path is the same, don't do anything
            _dialoguePath = path;
            Debug.Log($"DialogueRunner : Building dialogue tree from '{_dialoguePath}'"); // Log the path of the dialogue tree
            string actualPath = path.Replace("Assets/Resources/", "");
            actualPath = actualPath.Replace(".txt", ""); // Remove the .txt extension
            // Initialize and load dialogue tree
            if (!string.IsNullOrEmpty(actualPath))
            {
                _dialogueTree = new DialogueTree(actualPath);
                _currentNode = _dialogueTree.Root;
            }
        }

        public IEnumerator RunDialogueCoroutine()
        {
            if (_dialogueTree == null || DialogueFrontend == null)
            {
                Debug.LogError("DialogueTree or DialogueFrontend is not set.");
                yield break;
            }

            Debug.Log("Beginning Dialogue");
            Task beginDialogueTask = DialogueFrontend.BeginDialogue(_characterName);
            while (!beginDialogueTask.IsCompleted) yield return null;

            Debug.Log("Running Dialogue");
            Task dialogueTask = TraverseDialogue(_currentNode);
            while (!dialogueTask.IsCompleted) yield return null;

            Debug.Log("Ending Dialogue");
            Task endDialogueTask = DialogueFrontend.EndDialogue();
            while (!endDialogueTask.IsCompleted) yield return null;

            yield return null; // wait for the next frame

        }

        private async Task TraverseDialogue(DialogueNode node)
        {
            if (node == null) return;

            await Task.Yield(); // a frame between traversals 
            int choiceIndex = -1; // Default value when there are no choices
            Debug.Log($"Traversing dialogue node -- {node.Content}");

            foreach (DialogueNode child in node.Children)
                Debug.Log("Children: " + child.Content);

            // If there are choices, follow the choice path, otherwise, iterate through all children
            switch (node.Type)
            {
                case DialogueNode.NodeType.Branch:
                    Debug.Log("Displaying dialogue node -- branch");
                    choiceIndex = await DialogueFrontend.DisplayNode(node);
                    if (choiceIndex == -1)
                    {
                        Debug.LogError("No valid option was chosen!");
                    }
                    await TraverseDialogue(node.Children[choiceIndex]);
                    break;
                case DialogueNode.NodeType.Option:
                    Debug.Log("Displaying dialogue node -- option");
                    await TraverseDialogue(node.Children[0]); // don't display this node, just move on
                    break;
                case DialogueNode.NodeType.Event:
                    Debug.Log("Displaying dialogue node -- event");
                    // we should fire off an event here
                    // TODO: implement event firing
                    // event name
                    string eventName = node.Parameters[0] as string;
                    // event parameters
                    object[] parameters = new object[node.Parameters.Count - 1];
                    if (parameters.Length > 0)
                    {
                        for (int i = 1; i < node.Parameters.Count; i++)
                        {
                            parameters[i - 1] = node.Parameters[i];
                        }
                    }
                    Debug.Log("Firing DialogueEvent: " + eventName + " with parameters: " + parameters.Length + " parameters.");
                    OnDialogueEvent?.Invoke(eventName, parameters);
                    await TraverseDialogue(node.Children[0]); // continue on
                    // choiceIndex = await dialogueFrontend.DisplayNode(node, _characterName);
                    // await TraverseDialogue(node.Children[0]);
                    break;
                case DialogueNode.NodeType.Exit:
                    Debug.Log("Displaying dialogue node -- exit");
                    choiceIndex = await DialogueFrontend.DisplayNode(node);
                    await TraverseDialogue(node.Children[0]); // just go on to the next node
                    break;
                default:
                    Debug.Log("Displaying dialogue node -- standard text");
                    choiceIndex = await DialogueFrontend.DisplayNode(node);
                    await TraverseDialogue(node.Children[0]);
                    break;
            }
        }
    }
}
