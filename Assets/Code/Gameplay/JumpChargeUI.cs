using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ascendead.Tracking;

namespace Ascendead.UI
{
    public class JumpChargeUI : MonoBehaviour
    {
        [SerializeField] private GameObject _chargePrefab;
        [SerializeField] private Color _chargedColor = Color.white;
        [SerializeField] private Color _unchargedColor = Color.black;
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;

        private List<GameObject> _charges = new List<GameObject>();
        private static JumpChargeUI _instance;
        private int _maxCharges = 0;
        private int _currentChargeLevel = 0;

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            _maxCharges = ChargeTracker.GetMaxCharges();
            UpdgradeChargeLevel(_maxCharges);
        }

        public static void SetChargeLevel(int chargeLevel)
        {
            if (_instance == null) return;
            _instance._currentChargeLevel = chargeLevel;
            for (int i = 0; i < _instance._charges.Count; i++)
            {
                if (i < chargeLevel)
                {
                    _instance._charges[i].GetComponent<Image>().color = _instance._chargedColor;
                }
                else
                {
                    _instance._charges[i].GetComponent<Image>().color = _instance._unchargedColor;
                }
            }
        }

        public static void UpdgradeChargeLevel(int newMax)
        {
            if (_instance == null) return;

            _instance._maxCharges = newMax;
            // delete old charges
            for (int i = 0; i < _instance._charges.Count; i++)
            {
                Destroy(_instance._charges[i]);
            }
            _instance._charges.Clear();

            // respawn charges
            for (int i = 0; i < _instance._maxCharges; i++)
            {
                GameObject charge = Instantiate(_instance._chargePrefab, _instance._layoutGroup.transform);
                _instance._charges.Add(charge);
            }

            SetChargeLevel(_instance._currentChargeLevel);
        }
    }
}
