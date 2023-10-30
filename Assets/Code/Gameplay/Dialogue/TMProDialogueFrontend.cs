using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using CurlyCore.Input;
using CurlyUtility;
using CurlyCore;
using UnityEngine.InputSystem;
using UnityEditor.Experimental.GraphView;

namespace Ascendead.Dialogue
{
    [CreateAssetMenu(menuName = "Ascendead/Dialogue Frontend/TMPro", fileName = "TMProDialogueFrontend")]
    public class TMPDialogueFrontend : DialogueFrontendObject
    {
        [SerializeField] private GameObject _dialoguePanelPrefab;
        [SerializeField] private GameObject _choiceButtonPrefab;
        [SerializeField] private Transform _canvasTransform; // Assign the canvas transform
        [SerializeField, InputPath] private string _continuePrompt;
        [SerializeField] private float _typwriterSpeed = 10f; // Characters per second

        private GameObject _dialoguePanelInstance;
        private TextMeshProUGUI _dialogueText;
        private VerticalLayoutGroup _choicesLayoutGroup;
        [GlobalDefault] private InputManager _inputManager;

        private TaskCompletionSource<int> choiceMadeCompletionSource;

        public override async Task BeginDialogue()
        {
            if (_inputManager == null) DependencyInjector.InjectDependencies(this);

            _canvasTransform = GameObject.FindGameObjectWithTag("GameHUD").transform;

            if (_canvasTransform == null)
            {
                Debug.LogError("Canvas transform is not set.");
                return;
            }

            _dialoguePanelInstance = GameObject.Instantiate(_dialoguePanelPrefab, _canvasTransform);
            _dialogueText = _dialoguePanelInstance.GetComponentInChildren<TextMeshProUGUI>();
            _choicesLayoutGroup = _dialoguePanelInstance.GetComponentInChildren<VerticalLayoutGroup>();

            // Animate panel entering screen
            // ... (Implement the desired animation logic here)

            await Task.Yield();
        }

        public override async Task<int> DisplayNode(DialogueNode node, string characterName)
        {
            switch (node.Type)
            {
                case DialogueNode.NodeType.Content:
                    // just display the content
                    HideOptions();
                    await Typewriter.ApplyTo(_dialogueText, $"{characterName}: {node.Content}", _typwriterSpeed, inputManager: _inputManager, inputPrompt: _continuePrompt);
                    // Wait for continue prompt
                    await TaskUtility.WaitUntil(() =>
                    {
                        // Debug.Log("Waiting for continue prompt");
                        return _inputManager.GetInputDown(_continuePrompt);
                    });
                    break;
                case DialogueNode.NodeType.Branch:
                    // keep the text on screen, but show the options
                    Debug.Log("TMProDialogueFrontend : Displaying Branch!");
                    List<object> options = node.Parameters;
                    
                    Debug.Log($"Branch : Showing {options.Count} options!");
                    // show the layout group
                    _choicesLayoutGroup.gameObject.SetActive(true);

                    for (int i = 0; i < options.Count; i++)
                    {
                        var choice = GameObject.Instantiate(_choiceButtonPrefab, _choicesLayoutGroup.transform);
                        var choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
                        var button = choice.GetComponent<Button>();

                        choiceText.text = (string)options[i];
                        int choiceIndex = i;
                        button.onClick.AddListener(() => ChooseOption(choiceIndex));
                    }

                    choiceMadeCompletionSource = new TaskCompletionSource<int>();
                    return await choiceMadeCompletionSource.Task;
                case DialogueNode.NodeType.Option:
                    // we should never get here
                    break;
                case DialogueNode.NodeType.Event:
                    // we should never get here
                    break;
                case DialogueNode.NodeType.Exit:
                    // we should also never get here
                    break;
            }
            return await Task.FromResult(-1);
        }

        private void HideOptions()
        {
            // delete all the buttons
            foreach (Transform child in _choicesLayoutGroup.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            // hide the layout group
            _choicesLayoutGroup.gameObject.SetActive(false);
        }


        private void ChooseOption(int choiceIndex)
        {
            choiceMadeCompletionSource.TrySetResult(choiceIndex);
        }

        public override async Task EndDialogue()
        {
            // Animate panel exiting screen
            // ... (Implement the desired animation logic here)
            GameObject.Destroy(_dialoguePanelInstance);
            await Task.Yield();
        }
    }
}
