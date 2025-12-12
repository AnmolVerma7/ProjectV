using Antigravity.Controllers;
using KinematicCharacterController;
using UnityEngine;

namespace Antigravity.Movement
{
    /// <summary>
    /// Encapsulates all slide-related logic and state.
    /// Handles entry/exit conditions, cooldown, and surface-aware physics.
    /// </summary>
    public class SlideHandler
    {
        private readonly KinematicCharacterMotor _motor;
        private readonly PlayerMovementConfig _config;
        private readonly PlayerInputHandler _input;

        // State
        private bool _isSliding;
        private float _slideTimer;
        private Vector3 _slideDirection;
        private bool _pendingSlideEntry;
        private float _lastSlideExitTime = -999f; // Allow immediate slide on start

        // Dependencies (injected)
        private readonly System.Func<bool> _isCrouchingGetter;
        private readonly System.Action _enterCrouchAction;
        private readonly System.Action _tryUncrouchAction;

        /// <summary>
        /// Whether the player is currently sliding.
        /// </summary>
        public bool IsSliding => _isSliding;

        public SlideHandler(
            KinematicCharacterMotor motor,
            PlayerMovementConfig config,
            PlayerInputHandler input,
            System.Func<bool> isCrouchingGetter,
            System.Action enterCrouchAction,
            System.Action tryUncrouchAction
        )
        {
            _motor = motor;
            _config = config;
            _input = input;
            _isCrouchingGetter = isCrouchingGetter;
            _enterCrouchAction = enterCrouchAction;
            _tryUncrouchAction = tryUncrouchAction;
        }

        /// <summary>
        /// Resets slide state when module is activated.
        /// </summary>
        public void OnActivated()
        {
            _isSliding = false;
            _pendingSlideEntry = false;
            _slideTimer = 0f;
        }

        /// <summary>
        /// Called by PlayerController when crouch is activated.
        /// Requests slide entry (will be processed in HandleSlide).
        /// </summary>
        public void RequestSlide()
        {
            _pendingSlideEntry = true;
        }

        /// <summary>
        /// Manages slide entry/exit based on input state transitions.
        /// Called from AfterUpdate.
        /// </summary>
        public void HandleSlide()
        {
            // Capture pending request and reset flag immediately
            bool requestedSlide = _pendingSlideEntry;
            _pendingSlideEntry = false;

            // 1. Input-based Entry/Exit Triggers
            if (requestedSlide)
            {
                if (!_isSliding)
                {
                    TryEnterSlide();
                }
                else if (_config.ToggleSlide)
                {
                    // Toggle Mode: Pressing crouch again cancels slide
                    ExitSlide();
                    return; // Stop processing to prevent immediate re-entry checks
                }
            }

            // 2. Continuous State Checks (if still sliding)
            if (_isSliding)
            {
                // General Exit: Left Ground (Jumping/Falling)
                if (_motor.GroundingStatus.FoundAnyGround == false)
                {
                    ExitSlide();
                    return;
                }

                if (_config.ToggleSlide)
                {
                    // Toggle Mode: exits via crouch press (handled above) or speed loss
                }
                else
                {
                    // Hold Mode Exits:
                    // - Crouch Release
                    if (!_input.IsCrouching)
                    {
                        ExitSlide();
                        return;
                    }

                    // - Sprint Release (Optional for Hold Mode, keeps it responsive)
                    if (!_input.IsSprinting)
                    {
                        ExitSlide();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to enter slide state. Only triggers from Sprint + Crouch when moving.
        /// </summary>
        private bool TryEnterSlide()
        {
            float currentSpeed = _motor.Velocity.magnitude;
            bool canSlide =
                _input.IsSprinting
                && currentSpeed > 1f
                && _motor.GroundingStatus.IsStableOnGround
                && !_isCrouchingGetter()
                && (UnityEngine.Time.time >= _lastSlideExitTime + _config.SlideCooldown);

            if (canSlide)
            {
                _isSliding = true;
                _slideDirection = _motor.Velocity.normalized;
                _slideTimer = 0f;
                _enterCrouchAction();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Exits slide state. Stays crouched if user still holding crouch.
        /// </summary>
        private void ExitSlide()
        {
            _isSliding = false;
            _slideTimer = 0f;
            _lastSlideExitTime = UnityEngine.Time.time;

            if (!_input.IsCrouching)
                _tryUncrouchAction();
        }

        /// <summary>
        /// Applies surface-aware slide physics. Slopes modify speed.
        /// </summary>
        public void ApplySlidePhysics(ref Vector3 currentVelocity, float deltaTime)
        {
            _slideTimer += deltaTime;

            // Calculate slope influence on speed
            Vector3 groundNormal = _motor.GroundingStatus.GroundNormal;
            float slopeAngle = Vector3.Angle(groundNormal, _motor.CharacterUp);
            Vector3 slopeDirection = Vector3
                .ProjectOnPlane(-_motor.CharacterUp, groundNormal)
                .normalized;
            float slopeDot = Vector3.Dot(_slideDirection, slopeDirection);
            float slopeInfluence = slopeDot * (slopeAngle / 90f) * _config.SlideGravityInfluence;

            // Calculate and apply speed with friction
            float targetSpeed = Mathf.Max(_config.BaseSlideSpeed + slopeInfluence, 0f);
            float newSpeed = Mathf.Lerp(
                currentVelocity.magnitude,
                targetSpeed,
                _config.SlideFriction * deltaTime
            );

            // Exit conditions
            if (newSpeed < _config.MinSlideSpeedToMaintain)
            {
                ExitSlide();
                return;
            }
            if (_config.MaxSlideDuration > 0f && _slideTimer >= _config.MaxSlideDuration)
            {
                ExitSlide();
                return;
            }

            currentVelocity = _slideDirection * newSpeed;
        }
    }
}
