using System.Collections;
using System.Collections.Generic;
using Ascendead.Dialogue;
using Ascendead.Tracking;
using UnityEditor;
using UnityEngine;
using CurlyUtility;

namespace Ascendead.Components
{
    [RequireComponent(typeof(DialogueRunner))]
    public class Shopkeeper : MonoBehaviour
    {
        [SerializeField] private int _chargeUpgradeLevel = 2;
        [SerializeField] private int _chargeUpgradeCost = 5;

        [SerializeField, @FilePath] private string _enoughSoulsFilePath;
        [SerializeField, @FilePath] private string _notEnoughSoulsFilePath;
        [SerializeField, @FilePath] private string _alreadyBoughtChargeFilePath;

        private bool _hasBoughtCharge = false;


        private DialogueRunner _dialogueRunner;
        private const string UPGRADE_EVENT_NAME = "UpgradeCharge";

        private void Awake()
        {
            _dialogueRunner = GetComponent<DialogueRunner>();
            _dialogueRunner.OnDialogueEvent += HandleDialogueEvent;
            _dialogueRunner.SetDialogueTree(_notEnoughSoulsFilePath);
        }

        private void Update()
        {
            int charges = ChargeTracker.GetMaxCharges();
            if (_hasBoughtCharge) return;

            if (charges >= _chargeUpgradeLevel)
            {
                _dialogueRunner.SetDialogueTree(_alreadyBoughtChargeFilePath);
                _hasBoughtCharge = true;
            }

            int souls = SoulManager.GetSoulCount();
            if (souls >= _chargeUpgradeCost)
            {
                _dialogueRunner.SetDialogueTree(_enoughSoulsFilePath);
            }
            else
            {
                _dialogueRunner.SetDialogueTree(_notEnoughSoulsFilePath);
            }
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
            SoulManager.SpendSouls(_chargeUpgradeCost);
            ChargeTracker.SetMaxCharges(_chargeUpgradeLevel);
        }

        private void OnDestroy()
        {
            _dialogueRunner.OnDialogueEvent -= HandleDialogueEvent;
        }
    }
}
