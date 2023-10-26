using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CurlyCore.CurlyApp;
using CurlyCore.Input;
using CurlyUtility.DSA;
using CurlyCore;

namespace Ascendead.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [System.Serializable]
        public class PlayerContext
        {
            // Basic state fields
            public bool OnGround;
            public bool TouchingWall;

            // Input fields
            public Vector2 MovementInput;
            public bool JumpInput;

            // Configuration fields
            public PlayerConfiguration Configuration { get; private set; }

            // Cool reference fields
            public Rigidbody2D Rigidbody { get; private set; }

            public PlayerContext(PlayerConfiguration configuration, Rigidbody2D rigidbody)
            {
                Configuration = configuration;
                Rigidbody = rigidbody;
            }
        }

        [System.Serializable]
        public struct PlayerInputs
        {
            [field: GlobalDefault] public InputManager InputManager { get; private set; }
            [field: SerializeField, InputPath] public string MovementInput { get; private set; }
            [field: SerializeField, InputPath] public string JumpInput { get; private set; }
        }

        [System.Serializable]
        public struct PlayerConfiguration
        {
            [field: Header("Player Feel")]
            [field: SerializeField] public float MovementSpeed { get; private set; }
            [field: SerializeField] public float JumpForce { get; private set; }

            [field: Header("Observation Configuration")]
            [field: SerializeField] public float GroundCheckDistance { get; private set; }
        }

        public IState<PlayerContext> PlayerState => _playerStateMachine.NestedCurrentState();

        public Rigidbody2D Rigidbody { get; private set; }
        [field: SerializeField] private PlayerContext _context;
        [field: SerializeField] private PlayerInputs _playerInputs;
        [field: SerializeField] private PlayerConfiguration _playerConfiguration;
        [GlobalDefault] private InputManager _inputManager;
        private StateMachine<PlayerContext> _playerStateMachine;


        private void OnEnable()
        {
            CurlyCore.DependencyInjector.InjectDependencies(this);
            Rigidbody = GetComponent<Rigidbody2D>();
            _context = new PlayerContext(_playerConfiguration, Rigidbody);
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
            _playerStateMachine.OnLogic(_context);
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

            // Build ground state machine
            StateMachine<PlayerContext> groundStateMachine = new StateMachine<PlayerContext>();
            groundStateMachine.AddState(idleState);
            groundStateMachine.AddState(runState);
            groundStateMachine.AddState(jumpState);
            
            LambdaTransition<PlayerContext> idleToRunTransition = new LambdaTransition<PlayerContext>("Idle to Run");
            idleToRunTransition.SetTransitionCondition(x => x.MovementInput.magnitude > 0f);

            LambdaTransition<PlayerContext> runToIdleTransition = new LambdaTransition<PlayerContext>("Run to Idle");
            runToIdleTransition.SetTransitionCondition(x => x.MovementInput.magnitude == 0f);

            LambdaTransition<PlayerContext> toJump = new LambdaTransition<PlayerContext>("To Jump");
            toJump.SetTransitionCondition(x => x.JumpInput);

            groundStateMachine.AddTransition(idleState, runState, idleToRunTransition);
            groundStateMachine.AddTransition(runState, idleState, runToIdleTransition);
            groundStateMachine.AddTransition(idleState, jumpState, toJump);

            groundStateMachine.SetStartingState(idleState);

            return groundStateMachine;
        }

        private StateMachine<PlayerContext> BuildAirStateMachine()
        {
            PlayerFreefall freefallState = new PlayerFreefall();
            PlayerWallSlide wallSlideState = new PlayerWallSlide();

            StateMachine<PlayerContext> airStateMachine = new StateMachine<PlayerContext>();
            airStateMachine.AddState(freefallState);
            airStateMachine.AddState(wallSlideState);

            LambdaTransition<PlayerContext> toWallSlide = new LambdaTransition<PlayerContext>("To Wall Slide");
            toWallSlide.SetTransitionCondition(x => x.TouchingWall);

            LambdaTransition<PlayerContext> toFreefall = new LambdaTransition<PlayerContext>("To Freefall");
            toFreefall.SetTransitionCondition(x => !x.TouchingWall);

            airStateMachine.AddTransition(freefallState, wallSlideState, toWallSlide);
            airStateMachine.AddTransition(wallSlideState, freefallState, toFreefall);

            airStateMachine.SetStartingState(freefallState);

            return airStateMachine;
        }

        private void ObserveContext()
        {
            // Observe the context for changes
            _context.OnGround = Physics2D.Raycast(transform.position, Vector2.down, _context.Configuration.GroundCheckDistance);
        }

        private void GetInput()
        {
            // Get input from the input manager
            _context.MovementInput = _inputManager.ReadInput<Vector2>(_playerInputs.MovementInput);
            _context.JumpInput = _inputManager.ReadInput<bool>(_playerInputs.JumpInput);
        }
    }
}
