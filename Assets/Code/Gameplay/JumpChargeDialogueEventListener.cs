using System.Collections;
using System.Collections.Generic;
using Ascendead.Dialogue;
using Ascendead.Tracking;
using UnityEngine;

namespace Ascendead.Components
{
    [RequireComponent(typeof(DialogueRunner))]
    public class JumpChargeDialogueEventListener : MonoBehaviour
    {
        private DialogueRunner _dialogueRunner;
        private const string UPGRADE_EVENT_NAME = "UpgradeCharge";

        private void Awake()
        {
            _dialogueRunner = GetComponent<DialogueRunner>();
            _dialogueRunner.OnDialogueEvent += HandleDialogueEvent;
        }

        private void HandleDialogueEvent(string eventName, object[] parameters)
        {
            switch (eventName)
            {
                case UPGRADE_EVENT_NAME:
                    UpgradeEvent(parameters);
                    break;
                default:
                    break;
            }
        }

        private void UpgradeEvent(object[] parameters)
        {
            Debug.Log("upgrading charge level");
            if (parameters.Length != 1)
            {
                Debug.LogError("UpgradeEvent requires 1 parameter");
                return;
            }

            if (parameters[0] is int)
            {
                int newMax = (int)parameters[0];
                ChargeTracker.SetMaxCharges(newMax);
            }
            else
            {
                Debug.LogError("UpgradeEvent requires an int parameter");
            }
        }

        private void OnDestroy()
        {
            _dialogueRunner.OnDialogueEvent -= HandleDialogueEvent;
        }
    }
}
