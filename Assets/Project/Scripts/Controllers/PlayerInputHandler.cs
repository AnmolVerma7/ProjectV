using UnityEngine;

namespace Antigravity.Controllers
{
    /// <summary>
    /// Handles all input detection for the player.
    /// This script's ONLY job is to listen to the Input System and store the results.
    /// It does not decide *what* to do with the input (that's the Controller's job).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(InputRouter))]
    public class PlayerInputHandler : MonoBehaviour, IInputUser
    {
        #region Inspector Fields

        [Header("Movement Input")]
        [SerializeField]
        private Vector2 _moveInput;

        [Header("Look Input")]
        [SerializeField]
        private Vector2 _lookDelta; // Mouse

        [SerializeField]
        private Vector2 _lookRate; // Gamepad

        [Header("Button States")]
        [Tooltip("True while Jump button is held down")]
        [SerializeField]
        private bool _jumpPressed;

        [Tooltip("True while Crouch button is held down")]
        [SerializeField]
        private bool _crouchPressed;

        [Tooltip("True while Rewind/Shoot button is held down")]
        [SerializeField]
        private bool _rewindPressed;

        #endregion

        #region Private Fields (Not Shown in Inspector)

        // One-frame triggers - reset each frame, not useful to display
        private bool _jumpTriggered;
        private bool _noclipTriggered;

        #endregion

        #region Public Accessors

        /// <summary>Current movement input (WASD/Stick).</summary>
        public Vector2 MoveInput => _moveInput;

        /// <summary>Mouse delta this frame (pixels).</summary>
        public Vector2 LookDelta => _lookDelta;

        /// <summary>Gamepad stick rate (-1 to 1).</summary>
        public Vector2 LookRate => _lookRate;

        /// <summary>True for ONE frame when jump is first pressed.</summary>
        public bool JumpDown => _jumpTriggered;

        /// <summary>True while jump button is held.</summary>
        public bool JumpHeld => _jumpPressed;

        /// <summary>True while crouch button is held.</summary>
        public bool CrouchHeld => _crouchPressed;

        /// <summary>True while rewind button is held.</summary>
        public bool RewindHeld => _rewindPressed;

        /// <summary>True for ONE frame when noclip is toggled.</summary>
        public bool NoclipToggleDown => _noclipTriggered;

        #endregion

        /// <inheritdoc />
        public void RegisterInputs(InputBuilder builder)
        {
            // Move
            builder.Bind(builder.Actions.Move).To<Vector2>(v => _moveInput = v);

            // Look (Advanced: Handle Mouse vs Gamepad)
            // We bypass the builder slightly to use the specialized LookInputCommand
            var router = GetComponent<InputRouter>();
            if (router != null)
            {
                router.Bind(
                    builder.Actions.Look,
                    new LookInputCommand(
                        delta => _lookDelta += delta, // Accumulate delta for this frame
                        rate => _lookRate = rate // Set continuous rate
                    )
                );
            }

            // Jump
            builder
                .Bind(builder.Actions.Jump)
                .Press(() =>
                {
                    _jumpTriggered = true;
                    _jumpPressed = true;
                })
                .Release(() => _jumpPressed = false)
                .Register();

            // Crouch
            builder
                .Bind(builder.Actions.Crouch)
                .Press(() => _crouchPressed = true)
                .Release(() => _crouchPressed = false)
                .Register();

            // Rewind
            builder
                .Bind(builder.Actions.Shoot)
                .Hold(0.1f)
                .Press(() => _rewindPressed = true)
                .Release(() => _rewindPressed = false)
                .Register();
        }

        private void Awake()
        {
            // Self-Registration to avoid needing "UniversalInput" component
            var router = GetComponent<InputRouter>();
            if (router != null)
            {
                var builder = new InputBuilder(router);
                RegisterInputs(builder);
            }
        }

        private void OnEnable()
        {
            // Lock cursor on start
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Dev Inputs (Reverting to Legacy Input as 'Both' is enabled and it's reliable)
            if (UnityEngine.Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("Noclip Toggle Pressed");
                _noclipTriggered = true;
            }

            // Cursor Handling
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0) && Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void LateUpdate()
        {
            // Reset one-frame triggers and accumulators
            _jumpTriggered = false;
            _noclipTriggered = false;
            _lookDelta = Vector2.zero; // Reset accumulated mouse delta
        }
    }
}
