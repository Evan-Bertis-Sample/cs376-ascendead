using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ascendead.Tracking;
using UnityEngine.UI;
using TMPro;

namespace Ascendead.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SoulUI : MonoBehaviour
    {
        [field: SerializeField] private Image _soulIcon;
        [field: SerializeField] private TextMeshProUGUI _soulCountText;
        private int _soulCount;

        private void Start()
        {
            _soulCountText = GetComponent<TextMeshProUGUI>();
            SoulManager.OnSoulCollected += OnSoulCollect;
            SoulManager.OnSoulSpent += OnSoulSpent;
        }

        private void Update()
        {
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
            
        }
    }
}
