using System.Collections;
using System.Collections.Generic;
using Ascendead.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Ascendead.Components
{
    public class BasicSpriteAnimationPlayer : MonoBehaviour
    {
        [field: SerializeField] public SpriteAnimation Animation { get; private set; }
        [field: SerializeField] public bool RandomizeStartTime { get; private set; } = false;
        [field: SerializeField] public bool UpdateTime { get; private set; } = true;

        private SpriteRenderer _spriteRenderer;

        protected void Start()
        {
            float startTime = 0;

            if (Animation == null) throw new System.Exception("BasicSpriteAnimationPlayer has no animation assigned.");

            // copy the animation object
            Animation = Instantiate(Animation);

            if (RandomizeStartTime) startTime = Random.Range(0f, 1f);

            Animation.ResetAnimation(startTime);
        }

        private void Update()
        {
            if (Animation == null) return;
            if (UpdateTime) Animation.UpdateTime();

            Sprite sprite = Animation.GetFrame();
            if (sprite != null) SetSprite(sprite);
        }

        protected virtual void SetSprite(Sprite sprite)
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null) throw new System.Exception("BasicSpriteAnimationPlayer has no SpriteRenderer component.");
            _spriteRenderer.sprite = sprite;
        }
    }
}
