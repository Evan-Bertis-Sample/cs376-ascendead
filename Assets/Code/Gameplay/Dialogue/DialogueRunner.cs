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

        private DialogueTree _dialogueTree;
        private DialogueNode _currentNode;

        public DialogueFrontendObject dialogueFrontend;

        void Start()
        {
            Debug.Log($"DialogueRunner : Building dialogue tree from '{_dialoguePath}'"); // Log the path of the dialogue tree
            string actualPath = _dialoguePath.Replace("Assets/Resources/", "");
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
            if (_dialogueTree == null || dialogueFrontend == null)
            {
                Debug.LogError("DialogueTree or DialogueFrontend is not set.");
                yield break;
            }

            Debug.Log("Beginning Dialogue");
            Task beginDialogueTask = dialogueFrontend.BeginDialogue();
            while (!beginDialogueTask.IsCompleted) yield return null;

            Debug.Log("Running Dialogue");
            Task dialogueTask = TraverseDialogue(_currentNode);
            while (!dialogueTask.IsCompleted) yield return null;

            Debug.Log("Ending Dialogue");
            Task endDialogueTask = dialogueFrontend.EndDialogue();
            while (!endDialogueTask.IsCompleted) yield return null;

        }

        private async Task TraverseDialogue(DialogueNode node)
        {
            if (node == null) return;

            int choiceIndex = -1; // Default value when there are no choices

            if (!node.IsMeta)
            {
                Debug.Log("Displaying dialogue node -- standard text");
                choiceIndex = await dialogueFrontend.DisplayNode(node, _characterName);
                await TraverseDialogue(node.Children[0]);
            }

            // If there are choices, follow the choice path, otherwise, iterate through all children
            if (node.IsMeta && node.Content.Equals("Branch") && choiceIndex != -1 && node.Children.Count > choiceIndex)
            {
                // Traverse the path of the chosen option
                await TraverseDialogue(node.Children[choiceIndex]);
            }
        }
    }
}
