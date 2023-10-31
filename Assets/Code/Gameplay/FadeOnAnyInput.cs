using System.Collections;
using System.Collections.Generic;
using CurlyCore;
using CurlyCore.Input;
using UnityEngine;

namespace Ascendead.Components
{
    public class FadeOnAnyInput : MonoBehaviour
    {
        [SerializeField, InputPath] private string _anyInput;
        [SerializeField] private float _fadeTime = 1f;
        [SerializeField] private float _startingAlpha = 1f;
        [SerializeField] private float _endingAlpha = 0f;
        [SerializeField] private float _fadeDelay = 0.5f;

        [GlobalDefault] private InputManager _inputManager;
        private bool _fading = false;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            if (_inputManager == null) DependencyInjector.InjectDependencies(this);
            _canvasGroup = GetComponent<CanvasGroup>();
            _fading = false;
            _canvasGroup.alpha = _startingAlpha;
        }

        private void Update()
        {
            if (_fading) return;

            if (_inputManager.GetInputDown(_anyInput))
            {
                StartCoroutine(Fade());
            }
        }

        private IEnumerator Fade()
        {
            _fading = true;
            
            float time = 0f;

            while (time < _fadeDelay)
            {
                time += Time.deltaTime;
                yield return null;
            }

            time = 0f;

            while (time < _fadeTime)
            {
                time += Time.deltaTime;
                float t = time / _fadeTime;
                _canvasGroup.alpha = Mathf.Lerp(_startingAlpha, _endingAlpha, t);
                yield return null;
            }
        }
    }
}
