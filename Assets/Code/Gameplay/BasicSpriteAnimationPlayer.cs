using System.Collections;
using System.Collections.Generic;
using Ascendead.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Ascendead.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BasicSpriteAnimationPlayer : MonoBehaviour
    {
        [field: SerializeField] public SpriteAnimation Animation { get; private set; }

        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            Animation.ResetAnimation();
        }

        private void Update()
        {
            if (Animation == null) return;
            Animation.UpdateTime();

            Sprite sprite = Animation.GetFrame();
            if (sprite != null) _spriteRenderer.sprite = sprite;
        }
    }
}
