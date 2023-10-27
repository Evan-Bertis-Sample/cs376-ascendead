using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendead.Player
{
    [CreateAssetMenu(fileName = "SpriteAnimation", menuName = "Ascendead/Sprite Animation")]
    public class SpriteAnimation : ScriptableObject
    {
        [field: SerializeField] public List<Sprite> Frames { get; private set; }
        [field: SerializeField] public AnimationCurve FrameCurve { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [field: SerializeField] public bool Loop { get; private set; }
        [field: SerializeField] public int FramesPerSecond {get; private set;} = 12;

        private float _frameTime = 0f;

        private bool _isNotBoundToTime = false;
        private float _time = 0f;

        public void ResetAnimation()
        {
            _time = 0f;
        }

        public void BindTimeValue(float value, float minValue, float maxValue, bool clamp = true)
        {
            _isNotBoundToTime = true;
            if (clamp) value = Mathf.Clamp(value, minValue, maxValue);
            float t = Map(value, minValue, maxValue, 0f, 1f);
            _time = t;
            // Debug.Log($"Bound to time value, original value: {value}, mapped value: {t}");
        }

        public void RebindToTime(bool reset = true)
        {
            _isNotBoundToTime = false;
            if (reset) ResetAnimation();
        }

        public Sprite GetFrame()
        {
            if (Frames.Count == 0) return null;

            float frameIndex = FrameCurve.Evaluate(_time);
            int index = Mathf.RoundToInt(Mathf.Lerp(0, Frames.Count - 1, frameIndex));
            return Frames[index];
        }

        public void UpdateTime()
        {
            if (_isNotBoundToTime)
                return; // This is bound to a different type of variable, controlled by the user

            if (Frames.Count == 0) return;

            _frameTime = 1f / FramesPerSecond;
            float animationLength = Frames.Count * _frameTime;

            float progress = Mathf.InverseLerp(0, animationLength, Time.deltaTime);

            _time += progress;
            // _time += Time.deltaTime / _frameTime;
            _time %= 1f;
        }

        public float Map(float val, float in1, float in2, float out1, float out2)
        {
            return out1 + (val - in1) * (out2 - out1) / (in2 - in1);
        }
    }
}
