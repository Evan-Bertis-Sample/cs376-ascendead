using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CurlyCore.CurlyApp;
using CurlyCore.Input;
using CurlyUtility.DSA;
using CurlyCore;
using Unity.VisualScripting;
using CurlyCore.Audio;

namespace Ascendead.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [System.Serializable]
        public class PlayerContext
        {
            public string PlayerStateName;
            // Basic state fields
            public bool OnGround;
            public enum TouchingWall { None, Left, Right, Both }
            public TouchingWall OnWall;
            public float GroundCollisionVelocity;

            // Input fields
            public Vector2 MovementInput;
            public bool JumpInput;
            public float TimeHoldingJump;

            // Configuration fields
            public PlayerConfiguration Configuration { get; private set; }

            // Cool reference fields
            public Rigidbody2D Rigidbody { get; private set; }
            public GameObject PlayerObject { get; private set; }

            public void SetReferences(PlayerConfiguration configuration, Rigidbody2D rigidbody, GameObject playerObject)
            {
                Configuration = configuration;
                Rigidbody = rigidbody;
                PlayerObject = playerObject;
            }
        }

        [System.Serializable]
        public class PlayerInputs
        {
            [field: GlobalDefault] public InputManager InputManager { get; private set; }
            [field: SerializeField, InputPath] public string MovementInput { get; private set; }
            [field: SerializeField, InputPath] public string JumpInput { get; private set; }
        }

        [System.Serializable]
        public class PlayerConfiguration
        {
            [field: SerializeField] public float GroundMovementSpeed { get; private set; } = 5f;
            [field: SerializeField] public float AirMovementSpeed { get; private set; } = 3.5f;
            [field: SerializeField] public AnimationCurve AccelerationCurve { get; private set; } = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            [field: SerializeField] public float AccelerationTime { get; private set; } = 0.5f;
            [field: SerializeField] public float JumpForce { get; private set; } = 5f;
            [field: SerializeField] public float WallJumpForce { get; private set; } = 5f;
            [field: SerializeField] public float WallSlideFallReduction { get; private set; } = 0.25f;
            [field: SerializeField] public float JumpAngle { get; private set; } = 60f;
            [field: SerializeField] public float HardLandPunishmentTime { get; private set; } = 0.5f;

            [field: Header("Audio Clips")]
            [field: SerializeField, AudioPath] public string JumpAudio { get; private set; }
            [field: SerializeField, AudioPath] public string LandAudio { get; private set; }
            [field: SerializeField, AudioPath] public string WallJumpAudio { get; private set; }

            [field: Header("Observation Configuration")]
            [field: SerializeField] public float GroundCheckDistance { get; private set; } = 0.5f;
            [field: SerializeField] public LayerMask GroundLayer { get; private set; }
        }

        public IState<PlayerContext> PlayerState => _playerStateMachine.NestedCurrentState();

        public Rigidbody2D Rigidbody { get; private set; }
        [field: SerializeField] public PlayerContext Context { get; private set; }
        [field: SerializeField] public PlayerInputs InputSchema { get; private set; }
        [field: SerializeField] private PlayerConfiguration _playerConfiguration;
        [GlobalDefault] private InputManager _inputManager;
        private StateMachine<PlayerContext> _playerStateMachine;


        private void OnEnable()
        {
            CurlyCore.DependencyInjector.InjectDependencies(this);
            Rigidbody = GetComponent<Rigidbody2D>();
            Context.SetReferences(_playerConfiguration, Rigidbody, this.gameObject);
            BuildStateMachine();
        }

        private void Update()
        {
            if (_inputManager == null)
            {
                Debug.LogError("Input manager is null.");
                return;
            }
            ObserveContext();
            GetInput();
            _playerStateMachine.OnLogic(Context);
        }

        private void BuildStateMachine()
        {
            // Build ground state machine
            StateMachine<PlayerContext> groundStateMachine = BuildGroundStateMachine();
            StateMachine<PlayerContext> airStateMachine = BuildAirStateMachine();

            LambdaTransition<PlayerContext> groundToAirTransition = new LambdaTransition<PlayerContext>("Ground to Air");
            groundToAirTransition.SetTransitionCondition(x => !x.OnGround);

            LambdaTransition<PlayerContext> airToGroundTransition = new LambdaTransition<PlayerContext>("Air to Ground");
            airToGroundTransition.SetTransitionCondition(x => x.OnGround);

            // Build player state machine
            _playerStateMachine = new StateMachine<PlayerContext>();
            _playerStateMachine.AddState(groundStateMachine);
            _playerStateMachine.AddState(airStateMachine);

            _playerStateMachine.AddTransition(groundStateMachine, airStateMachine, groundToAirTransition);
            _playerStateMachine.AddTransition(airStateMachine, groundStateMachine, airToGroundTransition);

            _playerStateMachine.SetStartingState(groundStateMachine);
        }

        private StateMachine<PlayerContext> BuildGroundStateMachine()
        {
            // Create states
            PlayerIdle idleState = new PlayerIdle();
            PlayerRun runState = new PlayerRun();
            PlayerJump jumpState = new PlayerJump();
            PlayerLand landState = new PlayerLand();

            // Build ground state machine
            StateMachine<PlayerContext> groundStateMachine = new StateMachine<PlayerContext>();
            groundStateMachine.AddState(idleState);
            groundStateMachine.AddState(runState);
            groundStateMachine.AddState(jumpState);
            groundStateMachine.AddState(landState);

            LambdaTransition<PlayerContext> idleToRunTransition = new LambdaTransition<PlayerContext>("Idle to Run");
            idleToRunTransition.SetTransitionCondition(x => x.MovementInput.x != 0f);

            LambdaTransition<PlayerContext> runToIdleTransition = new LambdaTransition<PlayerContext>("Run to Idle");
            runToIdleTransition.SetTransitionCondition(x => x.MovementInput.x == 0f);

            LambdaTransition<PlayerContext> toJump = new LambdaTransition<PlayerContext>("To Jump");
            toJump.SetTransitionCondition(x => x.JumpInput);

            LambdaTransition<PlayerContext> jumpToLand = new LambdaTransition<PlayerContext>("Jump to Land");
            jumpToLand.SetTransitionCondition(x => true);

            LambdaTransition<PlayerContext> landToIdle = new LambdaTransition<PlayerContext>("Land to Idle");
            landToIdle.SetTransitionCondition(x => true);

            LambdaTransition<PlayerContext> idleToLand = new LambdaTransition<PlayerContext>("Idle to Land");
            idleToLand.SetTransitionCondition(x => x.GroundCollisionVelocity < landState.MinimumCollisionVelocity);

            groundStateMachine.AddTransition(idleState, runState, idleToRunTransition);
            groundStateMachine.AddTransition(runState, idleState, runToIdleTransition);
            groundStateMachine.AddTransition(idleState, jumpState, toJump);
            groundStateMachine.AddTransition(runState, jumpState, toJump);
            groundStateMachine.AddTransition(jumpState, landState, jumpToLand);
            groundStateMachine.AddTransition(landState, idleState, landToIdle);
            groundStateMachine.AddTransition(idleState, landState, idleToLand);

            groundStateMachine.SetStartingState(idleState);

            return groundStateMachine;
        }

        private StateMachine<PlayerContext> BuildAirStateMachine()
        {
            PlayerFreefall freefallState = new PlayerFreefall();
            PlayerWallSlide wallSlideState = new PlayerWallSlide();
            PlayerWallJump wallJumpState = new PlayerWallJump();

            StateMachine<PlayerContext> airStateMachine = new StateMachine<PlayerContext>();
            airStateMachine.AddState(freefallState);
            airStateMachine.AddState(wallSlideState);
            airStateMachine.AddState(wallJumpState);

            LambdaTransition<PlayerContext> toWallSlide = new LambdaTransition<PlayerContext>("To Wall Slide");
            toWallSlide.SetTransitionCondition(x => (x.OnWall == PlayerContext.TouchingWall.Left || x.OnWall == PlayerContext.TouchingWall.Right) && x.Rigidbody.velocity.y < 0f);

            LambdaTransition<PlayerContext> cancelWallSlide = new LambdaTransition<PlayerContext>("Cancel Wall Slide");
            cancelWallSlide.SetTransitionCondition(x => x.MovementInput.y < 0f || x.Rigidbody.velocity.y >= 0f);

            LambdaTransition<PlayerContext> toFreefall = new LambdaTransition<PlayerContext>("To Freefall");
            toFreefall.SetTransitionCondition(x => x.OnWall != PlayerContext.TouchingWall.Left &&  x.OnWall != PlayerContext.TouchingWall.Right);

            LambdaTransition<PlayerContext> wallSlideToWallJump = new LambdaTransition<PlayerContext>("Wall Slide to Wall Jump");
            wallSlideToWallJump.SetTransitionCondition(x => x.JumpInput && x.TimeHoldingJump == 0f);

            LambdaTransition<PlayerContext> wallJumpToFreefall = new LambdaTransition<PlayerContext>("Wall Jump to Freefall");
            wallJumpToFreefall.SetTransitionCondition(x => true);

            airStateMachine.AddTransition(freefallState, wallSlideState, toWallSlide);
            airStateMachine.AddTransition(wallSlideState, freefallState, toFreefall);
            airStateMachine.AddTransition(wallSlideState, freefallState, cancelWallSlide);
            airStateMachine.AddTransition(wallSlideState, wallJumpState, wallSlideToWallJump);
            // airStateMachine.AddTransition(wallJumpState, freefallState, toFreefall);
            airStateMachine.AddTransition(wallJumpState, freefallState, wallJumpToFreefall);

            airStateMachine.SetStartingState(freefallState);

            return airStateMachine;
        }

        private void ObserveContext()
        {
            // Parameters
            Vector2 position = transform.position;
            int numRays = 3;  // Example count, adjust as needed
            float raySpacing = 0.5f; // Space between rays, adjust based on player size
            float rayLength = Context.Configuration.GroundCheckDistance;
            LayerMask groundLayer = Context.Configuration.GroundLayer;

            Context.OnGround = false;
            for (int i = 0; i < numRays; i++)
            {
                Vector2 rayStart = position + Vector2.left * raySpacing * (i - numRays / 2);
                if (Physics2D.Raycast(rayStart, Vector2.down, rayLength, groundLayer))
                {
                    Context.OnGround = true;
                    break;
                }

                // Debugging
                Debug.DrawLine(rayStart, rayStart + Vector2.down * rayLength, Context.OnGround ? Color.green : Color.red);
            }

            Context.PlayerStateName = PlayerState.GetType().Name;

            if (Context.JumpInput) Context.TimeHoldingJump += Time.deltaTime;
            else Context.TimeHoldingJump = 0f;

            // Logic for wall detection
            Context.OnWall = PlayerContext.TouchingWall.None;
            if (Physics2D.Raycast(position, Vector2.left, rayLength, groundLayer))
            {
                Context.OnWall = PlayerContext.TouchingWall.Left;
            }
            if (Physics2D.Raycast(position, Vector2.right, rayLength, groundLayer))
            {
                if (Context.OnWall == PlayerContext.TouchingWall.Left)
                    Context.OnWall = PlayerContext.TouchingWall.Both;
                else
                    Context.OnWall = PlayerContext.TouchingWall.Right;
            }
        }

        private void GetInput()
        {
            // Get input from the input manager
            Context.MovementInput = _inputManager.ReadInput<Vector2>(InputSchema.MovementInput);
            Context.JumpInput = _inputManager.ReadInput<float>(InputSchema.JumpInput) > 0;
        }

        /// <summary>
        /// Sent when an incoming collider makes contact with this object's
        /// collider (2D physics only).
        /// </summary>
        /// <param name="other">The Collision2D data associated with this collision.</param>
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Context.GroundCollisionVelocity = -other.relativeVelocity.y;
            }
        }
    }
}
