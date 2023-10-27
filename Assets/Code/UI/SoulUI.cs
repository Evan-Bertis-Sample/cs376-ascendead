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
        [field: SerializeField] private float _colorChangeSpeed = 10f;

        private int _soulCount;
        private Color _goalColor = Color.white;

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
            UpdateIconColor();
        }

        private void UpdateIconColor()
        {
            if (_soulIcon == null) return;
            _soulIcon.color = Color.Lerp(_soulIcon.color, _goalColor, _colorChangeSpeed * Time.deltaTime);

            // slowly always return to white
            _goalColor = Color.Lerp(_goalColor, Color.white, _colorChangeSpeed * Time.deltaTime);
        }

        private void OnSoulCollect(int soulAmount)
        {
            _goalColor = _collectColorModifier;   
        }

        private void OnSoulSpent(int soulAmount)
        {
            _goalColor = _spendColorModifier;
        }

        private void OnDestroy()
        {
            SoulManager.OnSoulCollected -= OnSoulCollect;
            SoulManager.OnSoulSpent -= OnSoulSpent;
        }
    }
}
