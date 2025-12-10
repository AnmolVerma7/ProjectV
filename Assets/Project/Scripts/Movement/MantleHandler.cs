using Antigravity.Controllers;
using KinematicCharacterController;
using UnityEngine;

namespace Antigravity.Movement
{
    public enum MantleState
    {
        None,
        Grabbing,
        Hanging,
        Mantling
    }

    /// <summary>
    /// Encapsulates ledge grab and mantle logic using manual raycast-based detection.
    /// Provides parkour-style climbing without trigger colliders.
    /// </summary>
    public class MantleHandler
    {
        private readonly KinematicCharacterMotor _motor;
        private readonly PlayerMovementConfig _config;

        // State
        private MantleState _state = MantleState.None;
        private Vector3 _grabPosition;
        private Vector3 _mantleTargetPosition;
        private float _stateTimer;
        private bool _mantleRequested; // Jump input to confirm mantle

        // Properties
        public bool IsActive => _state != MantleState.None;
        public MantleState CurrentState => _state;

        public MantleHandler(KinematicCharacterMotor motor, PlayerMovementConfig config)
        {
            _motor = motor;
            _config = config;
        }

        /// <summary>
        /// Resets mantle state when module is activated.
        /// </summary>
        public void OnActivated()
        {
            _state = MantleState.None;
            _stateTimer = 0f;
            _mantleRequested = false;
        }

        /// <summary>
        /// Called when jump is pressed - used to confirm mantle while hanging.
        /// </summary>
        public void RequestMantle()
        {
            _mantleRequested = true;
        }

        /// <summary>
        /// Checks if the player can currently grab a ledge using manual detection.
        /// </summary>
        public bool CanGrab()
        {
            // Must not be already mantling
            if (IsActive)
            {
                // Debug.Log("Mantle: Already active");
                return false;
            }

            // Must be in air (not grounded)
            if (_motor.GroundingStatus.IsStableOnGround)
            {
                // Debug.Log("Mantle: Grounded");
                return false;
            }

            // Use character forward instead of velocity direction
            // This allows mantling even when jumping straight up at a wall
            Vector3 forwardDirection = _motor.CharacterForward;

            // Check for wall in front
            RaycastHit wallHit;
            if (
                !Physics.Raycast(
                    _motor.TransientPosition,
                    forwardDirection,
                    out wallHit,
                    _config.MaxGrabDistance,
                    _config.MantleLayers,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                // Debug.Log("Mantle: No wall detected");
                return false;
            }

            // Wall must be roughly vertical (not a slope or ceiling)
            // Vertical wall = 90° angle between normal and CharacterUp
            // Accept walls between 60° and 120° (within 30° of vertical)
            float wallAngle = Vector3.Angle(wallHit.normal, _motor.CharacterUp);
            if (wallAngle < 60f || wallAngle > 120f)
            {
                // Debug.Log($"Mantle: Wall not vertical enough ({wallAngle:F1}°)");
                return false;
            }

            // Check for ledge above (flat surface we can stand on)
            Vector3 ledgeCheckStart = wallHit.point + (_motor.CharacterUp * _config.MaxLedgeHeight);
            RaycastHit ledgeHit;

            if (
                !Physics.Raycast(
                    ledgeCheckStart,
                    -_motor.CharacterUp,
                    out ledgeHit,
                    _config.MaxLedgeHeight - _config.MinLedgeHeight,
                    _config.MantleLayers,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                // Debug.Log("Mantle: No ledge top found");
                return false;
            }

            // Ledge must be walkable (check surface angle)
            float surfaceAngle = Vector3.Angle(ledgeHit.normal, _motor.CharacterUp);
            if (surfaceAngle > 45f) // Max 45 degree slope
            {
                // Debug.Log($"Mantle: Surface too steep ({surfaceAngle:F1}°)");
                return false;
            }

            // Valid ledge found!
            // Debug.Log("✅ Mantle: VALID LEDGE DETECTED!");
            return true;
        }

        /// <summary>
        /// Attempts to grab the detected ledge.
        /// </summary>
        public void TryGrab()
        {
            if (!CanGrab())
                return;

            // Use character forward direction
            Vector3 forwardDirection = _motor.CharacterForward;

            // Find wall
            RaycastHit wallHit;
            Physics.Raycast(
                _motor.TransientPosition,
                forwardDirection,
                out wallHit,
                _config.MaxGrabDistance,
                _config.MantleLayers,
                QueryTriggerInteraction.Ignore
            );

            // Find ledge top
            Vector3 ledgeCheckStart = wallHit.point + (_motor.CharacterUp * _config.MaxLedgeHeight);
            RaycastHit ledgeHit;
            Physics.Raycast(
                ledgeCheckStart,
                -_motor.CharacterUp,
                out ledgeHit,
                _config.MaxLedgeHeight - _config.MinLedgeHeight,
                _config.MantleLayers,
                QueryTriggerInteraction.Ignore
            );

            // Calculate grab position (hanging below ledge)
            _grabPosition = ledgeHit.point;
            _grabPosition.y -= _motor.Capsule.height * 0.5f; // Hang at half-height below ledge
            _grabPosition -= forwardDirection * (_motor.Capsule.radius + 0.05f); // Pull back slightly

            // Calculate mantle target (on top of ledge)
            _mantleTargetPosition = ledgeHit.point;
            _mantleTargetPosition += forwardDirection * (_motor.Capsule.radius + 0.15f); // Move forward

            // Enter grabbing state
            _state = MantleState.Grabbing;
            _stateTimer = 0f;
        }

        /// <summary>
        /// Cancels the current mantle (e.g., player pressed jump to drop).
        /// </summary>
        public void CancelGrab()
        {
            if (_state == MantleState.Hanging || _state == MantleState.Grabbing)
            {
                _state = MantleState.None;
                _stateTimer = 0f;
            }
        }

        /// <summary>
        /// Updates mantle state machine.
        /// Called from AfterUpdate.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!IsActive)
                return;

            _stateTimer += deltaTime;

            switch (_state)
            {
                case MantleState.Grabbing:
                    UpdateGrabbing(deltaTime);
                    break;

                case MantleState.Hanging:
                    UpdateHanging(deltaTime);
                    break;

                case MantleState.Mantling:
                    UpdateMantling(deltaTime);
                    break;
            }
        }

        private void UpdateGrabbing(float deltaTime)
        {
            // Snap to grab position and transition to hanging
            _motor.MoveCharacter(_grabPosition);
            _state = MantleState.Hanging;
            _stateTimer = 0f;
        }

        private void UpdateHanging(float deltaTime)
        {
            // Only mantle when player presses jump (via RequestMantle)
            if (_mantleRequested)
            {
                _state = MantleState.Mantling;
                _stateTimer = 0f;
                _mantleRequested = false;
            }
        }

        private void UpdateMantling(float deltaTime)
        {
            // Calculate lerp progress using animation curve
            float progress = _stateTimer / _config.MantleDuration;

            if (progress >= 1f)
            {
                // Mantle complete - use MoveCharacter for final position
                _motor.MoveCharacter(_mantleTargetPosition);
                _state = MantleState.None;
                return;
            }

            // Evaluate curve for smooth motion
            float curveValue = _config.MantleCurve.Evaluate(progress);

            // Arc-based motion: Move UP first (0-60% of motion), then FORWARD (40-100%)
            // This creates a natural "pull up then climb over" feel like Fortnite

            // Vertical progress: accelerated (completes by ~70% of total time)
            float verticalProgress = Mathf.Clamp01(curveValue * 1.4f);

            // Horizontal progress: delayed start (begins at ~50% of total time) and smoothed
            float horizontalRaw = Mathf.Clamp01((curveValue - 0.5f) / 0.5f);
            // Apply smoothstep for eased horizontal motion
            float horizontalProgress = horizontalRaw * horizontalRaw * (3f - 2f * horizontalRaw);

            // Calculate vertical position (from grab Y to target Y)
            float currentY = Mathf.Lerp(_grabPosition.y, _mantleTargetPosition.y, verticalProgress);

            // Calculate horizontal position (from grab XZ to target XZ)
            float currentX = Mathf.Lerp(
                _grabPosition.x,
                _mantleTargetPosition.x,
                horizontalProgress
            );
            float currentZ = Mathf.Lerp(
                _grabPosition.z,
                _mantleTargetPosition.z,
                horizontalProgress
            );

            Vector3 targetPosition = new Vector3(currentX, currentY, currentZ);

            // Use MoveCharacter for collision detection
            _motor.MoveCharacter(targetPosition);
        }

        /// <summary>
        /// Overrides velocity when mantling to prevent physics interference.
        /// </summary>
        public void OverrideVelocity(ref Vector3 velocity)
        {
            if (IsActive)
            {
                // Zero out velocity while mantling
                velocity = Vector3.zero;
            }
        }
    }
}
