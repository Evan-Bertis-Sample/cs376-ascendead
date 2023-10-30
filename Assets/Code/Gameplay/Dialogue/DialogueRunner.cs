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
                    choiceIndex = await dialogueFrontend.DisplayNode(node, _characterName);
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
                    await TraverseDialogue(node.Children[0]); // continue on
                    // choiceIndex = await dialogueFrontend.DisplayNode(node, _characterName);
                    // await TraverseDialogue(node.Children[0]);
                    break;
                case DialogueNode.NodeType.Exit:
                    Debug.Log("Displaying dialogue node -- exit");
                    choiceIndex = await dialogueFrontend.DisplayNode(node, _characterName);
                    await TraverseDialogue(node.Children[0]); // just go on to the next node
                    break;
                default:
                    Debug.Log("Displaying dialogue node -- standard text");
                    choiceIndex = await dialogueFrontend.DisplayNode(node, _characterName);
                    await TraverseDialogue(node.Children[0]);
                    break;
            }
        }
    }
}
