using Antigravity.Controllers;
using KinematicCharacterController;
using UnityEngine;

namespace Antigravity.Movement
{
    /// <summary>
    /// Encapsulates all jump-related logic and state.
    /// Handles Pre-jump buffering, Coyote time, Double jumps, and Wall jumps.
    /// </summary>
    public class JumpHandler
    {
        private readonly KinematicCharacterMotor _motor;
        private readonly PlayerMovementConfig _config;

        // State
        private bool _jumpRequested;
        private bool _jumpConsumed;
        private bool _doubleJumpConsumed;
        private bool _jumpedThisFrame;
        private bool _canWallJump;
        private Vector3 _wallJumpNormal;

        // Timers
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump;

        public JumpHandler(KinematicCharacterMotor motor, PlayerMovementConfig config)
        {
            _motor = motor;
            _config = config;
        }

        public void OnActivated()
        {
            _jumpRequested = false;
            _jumpConsumed = false;
            _doubleJumpConsumed = false;
            _timeSinceJumpRequested = Mathf.Infinity;
            _timeSinceLastAbleToJump = 0f;
        }

        /// <summary>
        /// Signal that a jump input occurred.
        /// </summary>
        public void RequestJump()
        {
            _timeSinceJumpRequested = 0f;
            _jumpRequested = true;
        }

        /// <summary>
        /// Signal that a wall was hit (for wall jumps).
        /// </summary>
        public void OnWallHit(Vector3 wallNormal)
        {
            if (_config.AllowWallJump && !_motor.GroundingStatus.IsStableOnGround)
            {
                _canWallJump = true;
                _wallJumpNormal = wallNormal;
            }
        }

        /// <summary>
        /// Attempts to apply a jump force to velocity if conditions are met.
        /// </summary>
        public void ProcessJump(ref Vector3 currentVelocity, float deltaTime)
        {
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;

            // If no request, nothing to do
            if (!_jumpRequested)
                return;

            // 1. Try Double Jump
            if (TryDoubleJump(ref currentVelocity))
                return;

            // 2. Try Regular/Wall/Coyote Jump
            TryRegularJump(ref currentVelocity);
        }

        /// <summary>
        /// Cleanup and state resets (Coyote time, Jump buffer expiration).
        /// </summary>
        public void PostUpdate(float deltaTime)
        {
            _canWallJump = false; // Reset wall jump opportunity every frame

            // Buffer Expiration
            if (_jumpRequested && _timeSinceJumpRequested > _config.JumpPreGroundingGraceTime)
            {
                _jumpRequested = false;
            }

            // Coyote Time & Reset Consumption
            bool isGrounded = _config.AllowJumpingWhenSliding
                ? _motor.GroundingStatus.FoundAnyGround
                : _motor.GroundingStatus.IsStableOnGround;

            if (isGrounded)
            {
                if (!_jumpedThisFrame)
                {
                    _doubleJumpConsumed = false;
                    _jumpConsumed = false;
                }
                _timeSinceLastAbleToJump = 0f;
            }
            else
            {
                _timeSinceLastAbleToJump += deltaTime;
            }
        }

        #region Internal Logic

        private bool TryDoubleJump(ref Vector3 currentVelocity)
        {
            if (!_config.AllowDoubleJump)
                return false;

            // Must currently be in air/unable to standard jump to double jump
            bool isInAir = _config.AllowJumpingWhenSliding
                ? !_motor.GroundingStatus.FoundAnyGround
                : !_motor.GroundingStatus.IsStableOnGround;

            // Logic: Must have used first jump, haven't used double, and be in air
            if (_jumpConsumed && !_doubleJumpConsumed && isInAir)
            {
                ExecuteJump(ref currentVelocity, _motor.CharacterUp);
                _doubleJumpConsumed = true;
                return true;
            }

            return false;
        }

        private void TryRegularJump(ref Vector3 currentVelocity)
        {
            // Valid if: Wall jump valid OR (Not consumed AND (Grounded OR Coyote Time))
            bool canGroundJump =
                !_jumpConsumed
                && (
                    (
                        _config.AllowJumpingWhenSliding
                            ? _motor.GroundingStatus.FoundAnyGround
                            : _motor.GroundingStatus.IsStableOnGround
                    )
                    || _timeSinceLastAbleToJump <= _config.JumpPostGroundingGraceTime
                );

            if (_canWallJump || canGroundJump)
            {
                Vector3 jumpDirection = DetermineJumpDirection();
                ExecuteJump(ref currentVelocity, jumpDirection);
                _jumpConsumed = true;
            }
        }

        private Vector3 DetermineJumpDirection()
        {
            if (_canWallJump)
                return _wallJumpNormal;

            if (_motor.GroundingStatus.FoundAnyGround && !_motor.GroundingStatus.IsStableOnGround)
            {
                return _motor.GroundingStatus.GroundNormal;
            }

            return _motor.CharacterUp;
        }

        private void ExecuteJump(ref Vector3 currentVelocity, Vector3 jumpDirection)
        {
            // Unground to prevent KCC from snapping back immediately
            _motor.ForceUnground(0.1f);

            // Add velocity
            currentVelocity +=
                (jumpDirection * _config.JumpSpeed)
                - Vector3.Project(currentVelocity, _motor.CharacterUp);

            _jumpRequested = false; // Consumed request
            _jumpedThisFrame = true;
        }

        #endregion
    }
}
