using System;
using System.Collections;
using System.Collections.Generic;
using CurlyUtility.DSA;
using UnityEngine;

using PlayerContext = Ascendead.Player.PlayerController.PlayerContext;

namespace Ascendead.Player
{
    public class PlayerIdle : IState<PlayerContext>
    {

    }

    public class PlayerRun : IState<PlayerContext>
    {
        private float _timeInState = 0f;

        public void OnStateEnter()
        {
            _timeInState = 0f;
        }

        public void OnLogic(PlayerContext context)
        {
            _timeInState += Time.deltaTime;
            Debug.Log("Moving player");
            Vector2 movementVector = context.MovementInput;
            float speed = GetMoveSpeed(context, _timeInState);

            float xSpeed = Mathf.Abs(context.Rigidbody.velocity.x) > speed ? context.Rigidbody.velocity.x : movementVector.x * speed; // Set to whichever is higher
            if (Mathf.Sign(xSpeed) != Mathf.Sign(movementVector.x)) xSpeed += movementVector.x * speed; // If the player is trying to move in the opposite direction, add the speed to the velocity
            context.Rigidbody.velocity = new Vector2(xSpeed, context.Rigidbody.velocity.y);
        }

        public virtual float GetMoveSpeed(PlayerContext context, float timeInState)
        {
            float speed = context.Configuration.GroundMovementSpeed;
            float acceleration = context.Configuration.AccelerationCurve.Evaluate(timeInState);

            return speed * acceleration;
        }
    }

    public class PlayerFreefall : PlayerRun // just a different type of run
    {
        public override float GetMoveSpeed(PlayerContext context, float timeInState)
        {
            float speed = context.Configuration.AirMovementSpeed;
            float acceleration = context.Configuration.AccelerationCurve.Evaluate(timeInState);

            return speed * acceleration;
        }
    }

    public class PlayerJump : IState<PlayerContext>
    {
        private float _timeInState = 0f;
        protected bool _isHoldingInput = false;

        protected PlayerContext _context;

        public void OnStateEnter()
        {
            _timeInState = 0f;
            _isHoldingInput = true;
        }

        public virtual bool IsReady()
        {
            return _isHoldingInput == false;
        }

        public void OnLogic(PlayerContext context)
        {
            _context = context;
            _isHoldingInput = _context.JumpInput;
            _timeInState += Time.deltaTime;
        }

        public void OnStateExit()
        {
            _isHoldingInput = false;
            Debug.Log("Jumping player");
            float jumpForce = GetJumpForce(_context, _timeInState);

            Vector2 jumpDirection = GetJumpDirection(_context);

            Vector2 newVelocity = _context.Rigidbody.velocity + jumpDirection * jumpForce;
            _context.Rigidbody.velocity = newVelocity;
        }

        public virtual float GetJumpForce(PlayerContext context, float timeInState)
        {
            return context.Configuration.JumpForce;
        }

        public virtual Vector2 GetJumpDirection(PlayerContext context)
        {
            if (context.MovementInput.x != 0f)
            {
                return GetNormalJumpVector(context, context.MovementInput.x < 0f);
            }

            return Vector2.up;
        }

        protected Vector2 GetNormalJumpVector(PlayerContext context, bool left)
        {
            float theta = context.Configuration.JumpAngle;
            float thetaRadians = theta * Mathf.Deg2Rad;
            float x = Mathf.Cos(thetaRadians);

            if (left) x *= -1f;

            float y = Mathf.Sin(thetaRadians);
            return new Vector2(x, y);
        }
    }

    public class PlayerWallJump : PlayerJump
    {
        public override bool IsReady()
        {
            return true; // we shouldn't hold for a wall jump
        }

        public override float GetJumpForce(PlayerContext context, float timeInState)
        {
            return context.Configuration.WallJumpForce;
        }

        public override Vector2 GetJumpDirection(PlayerContext context)
        {
            if (context.OnWall == PlayerContext.TouchingWall.None) return Vector2.up;
            if (context.OnGround) return Vector2.zero;
            switch (context.OnWall)
            {
                case PlayerContext.TouchingWall.Left:
                    return GetNormalJumpVector(context, false);
                case PlayerContext.TouchingWall.Right:
                    return GetNormalJumpVector(context, true);
                default:
                    return Vector2.zero;
            }
        }
    }

    public class PlayerWallSlide : IState<PlayerContext>
    {
        private Rigidbody2D _rigidbody;
        private bool _alreadyReducedGravity = false;
        private float _reductionAmount = 0f;

        public void OnStateEnter()
        {
            _alreadyReducedGravity = false;
        }

        public void OnLogic(PlayerContext context)
        {
            if (_alreadyReducedGravity) return;
            _reductionAmount = context.Configuration.WallSlideFallReduction;
            _rigidbody = context.Rigidbody;
            _rigidbody.gravityScale *= _reductionAmount;
            _alreadyReducedGravity = true;
        }

        public void OnStateExit()
        {
            if (_alreadyReducedGravity) _rigidbody.gravityScale /= _reductionAmount;
        }


    }

    public class PlayerLand : IState<PlayerContext>
    {

    }
}
