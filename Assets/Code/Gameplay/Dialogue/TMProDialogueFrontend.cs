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

            _canvasTransform = GameObject.FindObjectOfType<Canvas>()?.transform;
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
            // Display node content
            await Typewriter.ApplyTo(_dialogueText, $"{characterName}: {node.Content}", _typwriterSpeed, inputManager: _inputManager, inputPrompt: _continuePrompt);

            // Clear previous choices
            foreach (Transform child in _choicesLayoutGroup.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            if (node.IsMeta && node.Content.Equals("Branch"))
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    var choice = GameObject.Instantiate(_choiceButtonPrefab, _choicesLayoutGroup.transform);
                    var choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
                    var button = choice.GetComponent<Button>();

                    choiceText.text = node.Children[i].Content;
                    int choiceIndex = i;
                    button.onClick.AddListener(() => ChooseOption(choiceIndex));
                }

                choiceMadeCompletionSource = new TaskCompletionSource<int>();
                return await choiceMadeCompletionSource.Task;
            }
            else
            {   
                // Wait for continue prompt
                await TaskUtility.WaitUntil(() =>
                {
                    Debug.Log("Waiting for continue prompt");
                    return _inputManager.GetInputDown(_continuePrompt);
                });
            }

            return await Task.FromResult(-1);
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
