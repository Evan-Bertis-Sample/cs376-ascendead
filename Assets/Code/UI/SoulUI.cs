using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ascendead.Tracking;
using UnityEngine.UI;
using TMPro;

namespace Ascendead.UI
{
    public class SoulUI : MonoBehaviour
    {
        [field: SerializeField] private Image _soulIcon;
        [field: SerializeField] private TextMeshProUGUI _soulCountText;

        [field: SerializeField] private Color _collectColorModifier;
        [field: SerializeField] private Color _spendColorModifier;

        private int _soulCount;

        private void Start()
        {
            // _soulCountText = GetComponent<TextMeshProUGUI>();
            if (_soulCountText == null) throw new System.Exception("SoulUI has no TextMeshProUGUI component.");
            SoulManager.OnSoulCollected += OnSoulCollect;
            SoulManager.OnSoulSpent += OnSoulSpent;
        }

        private void Update()
        {
            if (_soulCountText == null) return;
            _soulCount = SoulManager.GetSoulCount();
            _soulCountText.text = _soulCount.ToString();
        }

        private void OnSoulCollect(int soulAmount)
        {
            
        }

        private void OnSoulSpent(int soulAmount)
        {

        }

        private void OnDestroy()
        {
            SoulManager.OnSoulCollected -= OnSoulCollect;
            SoulManager.OnSoulSpent -= OnSoulSpent;
        }
    }
}
