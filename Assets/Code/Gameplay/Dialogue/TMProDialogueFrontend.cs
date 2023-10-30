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
        [SerializeField] private TMPDialogueFrontendComponent _frontendComponent;
        [SerializeField] private GameObject _choiceButtonPrefab;
        [SerializeField] private Transform _canvasTransform; // Assign the canvas transform
        [SerializeField, InputPath] private string _continuePrompt;
        [SerializeField] private float _typwriterSpeed = 10f; // Characters per second
        [SerializeField] private Vector3 _panelEnterPosition;

        private TMPDialogueFrontendComponent _dialoguePanelInstance;
        private RectTransform _dialoguePanelRectTransform;
        [GlobalDefault] private InputManager _inputManager;

        private TaskCompletionSource<int> choiceMadeCompletionSource;

        public override async Task BeginDialogue(string characterName)
        {
            if (_inputManager == null) DependencyInjector.InjectDependencies(this);

            _canvasTransform = GameObject.FindGameObjectWithTag("GameHUD").transform;

            if (_canvasTransform == null)
            {
                Debug.LogError("Canvas transform is not set.");
                return;
            }

            _dialoguePanelInstance = GameObject.Instantiate(_frontendComponent, _canvasTransform);
            _dialoguePanelInstance.CharacterNameText.text = characterName;
            _dialoguePanelRectTransform = _dialoguePanelInstance.GetComponent<RectTransform>();
            _dialoguePanelRectTransform.localPosition = _panelEnterPosition;

            // Animate panel entering screen
            // ... (Implement the desired animation logic here)

            await Task.Yield();
        }

        public override async Task<int> DisplayNode(DialogueNode node)
        {
            switch (node.Type)
            {
                case DialogueNode.NodeType.Content:
                    // just display the content
                    HideOptions();
                    await Typewriter.ApplyTo(_dialoguePanelInstance.DialogueText, node.Content, _typwriterSpeed, inputManager: _inputManager, inputPrompt: _continuePrompt);
                    // Wait for continue prompt

                    if (node.Children.Count > 0 && node.Children[0].Type != DialogueNode.NodeType.Branch)
                    {
                        await TaskUtility.WaitUntil(() =>
                        {
                            // Debug.Log("Waiting for continue prompt");
                            return _inputManager.GetInputDown(_continuePrompt);
                        });
                    }
                    break;
                case DialogueNode.NodeType.Branch:
                    // keep the text on screen, but show the options
                    Debug.Log("TMProDialogueFrontend : Displaying Branch!");
                    List<DialogueNode> options = node.Children;

                    Debug.Log($"Branch : Showing {options.Count} options!");
                    // show the layout group
                    _dialoguePanelInstance.ChoicesLayoutGroup.gameObject.SetActive(true);

                    for (int i = 0; i < options.Count; i++)
                    {
                        var choice = GameObject.Instantiate(_choiceButtonPrefab, _dialoguePanelInstance.ChoicesLayoutGroup.transform);
                        var choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
                        var button = choice.GetComponent<Button>();

                        choiceText.text = options[i].Parameters[0] as string;
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
            foreach (Transform child in _dialoguePanelInstance.ChoicesLayoutGroup.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            // hide the layout group
            _dialoguePanelInstance.ChoicesLayoutGroup.gameObject.SetActive(false);
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
