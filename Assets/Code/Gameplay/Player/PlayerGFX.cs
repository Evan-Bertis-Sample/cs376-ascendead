using System;
using System.Collections;
using System.Collections.Generic;
using CurlyUtility.DSA;
using UnityEngine;

using PlayerContext = Ascendead.Player.PlayerController.PlayerContext;

namespace Ascendead.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerGFX : MonoBehaviour
    {
        [System.Serializable]
        public struct PlayerAnimations
        {
            [field: SerializeField] public SpriteAnimation Idle {get; private set;}
            [field: SerializeField] public SpriteAnimation Run {get; private set;}
            [field: SerializeField] public SpriteAnimation Jump {get; private set;}
            [field: SerializeField] public SpriteAnimation Fall {get; private set;}
            [field: SerializeField] public SpriteAnimation WallSlide {get; private set;}
            [field: SerializeField] public SpriteAnimation WallJump {get; private set;}
        }

        [field: SerializeField] public PlayerAnimations Animations {get; private set;}
        [field: SerializeField] public PlayerController Controller {get; private set;}

        private Dictionary<Type, SpriteAnimation> _STATE_ANIMATION_MAP;
        private SpriteAnimation _currentAnimation;
        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            if (Controller == null) throw new System.Exception("PlayerGFX requires a PlayerController to function.");
            CreateAnimationMap();
            UpdateSpecialBindings();
            _currentAnimation = Animations.Idle;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void CreateAnimationMap()
        {
            _STATE_ANIMATION_MAP = new Dictionary<Type, SpriteAnimation>()
            {
                { typeof(PlayerIdle), Animations.Idle },
                { typeof(PlayerRun), Animations.Run },
                { typeof(PlayerJump), Animations.Jump },
            };
        }

        private void UpdateSpecialBindings()
        {
            // use this to bind sprite animations to specific floating point values
            Animations.Jump.BindTimeValue(Controller.Rigidbody.velocity.y, 0f, 0f, false);
        }

        private void Update()
        {
            UpdateSpecialBindings();
            IState<PlayerContext> state = Controller.PlayerState;

            if (state == null) return;

            bool found = _STATE_ANIMATION_MAP.TryGetValue(state.GetType(), out SpriteAnimation animation);
            if (!found) animation = null;

            if (animation != _currentAnimation && animation != null)
            {
                animation.Play();
                _currentAnimation = animation;
            }

            if (_currentAnimation == null) return;

            _currentAnimation.UpdateTime();
            Sprite frame = _currentAnimation.GetFrame();

            if (frame != null) _spriteRenderer.sprite = frame;

        }

    }
}
