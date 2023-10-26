using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendead.Player
{
    [CreateAssetMenu(fileName = "SpriteAnimation", menuName = "Ascendead/Sprite Animation")]
    public class SpriteAnimation : ScriptableObject
    {
        [field: SerializeField] public List<Sprite> Frames { get; private set; }
        [field: SerializeField] public AnimationCurve FrameCurve { get; private set; }
        [field: SerializeField] public bool Loop { get; private set; }
        [field: SerializeField] public float FrameTime { get; private set; }

        private bool _isNotBoundToTime = false;
        private float _time = 0f;

        public void Play()
        {
            _time = 0f;
        }

        public void BindTimeValue(float value, float minValue, float maxValue, bool clamp = true)
        {
            _isNotBoundToTime = true;
            if (clamp) value = Mathf.Clamp(value, minValue, maxValue);
            float t = Map(value, minValue, maxValue, 0f, 1f);
            _time = t;
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
            {
                _time += Time.deltaTime / FrameTime;
                if (_time > 1f)
                {
                    if (Loop) _time = 0f;
                    else _time = 1f;
                }
            }
        }

        public float Map(float val, float in1, float in2, float out1, float out2)
        {
            return out1 + (val - in1) * (out2 - out1) / (in2 - in1);
        }
    }
}
