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
        [field: SerializeField] public bool RandomizeStartTime { get; private set; } = false;


        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
            Animation.UpdateTime();

            Sprite sprite = Animation.GetFrame();
            if (sprite != null) _spriteRenderer.sprite = sprite;
        }
    }
}
