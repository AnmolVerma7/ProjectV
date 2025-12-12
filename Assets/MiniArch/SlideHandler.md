```csharp
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
        private float _initialSlideSpeed; // Captured at slide start (with boost)
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
                && currentSpeed > _config.MinSlideEntrySpeed
                && _motor.GroundingStatus.IsStableOnGround
                && !_isCrouchingGetter()
                && (UnityEngine.Time.time >= _lastSlideExitTime + _config.SlideCooldown);

            if (canSlide)
            {
                _isSliding = true;
                _slideDirection = _motor.Velocity.normalized;
                _slideTimer = 0f;
                _initialSlideSpeed = currentSpeed * _config.SlideSpeedBoost;
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
        /// Applies momentum-based slide physics with friction curve and steering.
        /// </summary>
        public void ApplySlidePhysics(ref Vector3 currentVelocity, float deltaTime)
        {
            _slideTimer += deltaTime;

            // 1. Calculate slide progress and apply friction curve
            float slideProgress = Mathf.Clamp01(_slideTimer / _config.MaxSlideDuration);
            float frictionFactor = _config.SlideFrictionCurve.Evaluate(slideProgress);
            float currentSlideSpeed = _initialSlideSpeed * frictionFactor;

            // 2. Apply slope influence (faster downhill, slower uphill)
            if (_motor.GroundingStatus.FoundAnyGround)
            {
                Vector3 groundNormal = _motor.GroundingStatus.GroundNormal;
                Vector3 slopeDirection = Vector3
                    .ProjectOnPlane(-_motor.CharacterUp, groundNormal)
                    .normalized;
                float slopeFactor = Vector3.Dot(_slideDirection, slopeDirection);
                currentSlideSpeed += slopeFactor * _config.SlopeInfluence * 5f;
                currentSlideSpeed = Mathf.Max(currentSlideSpeed, _config.MinSlideExitSpeed * 0.8f);
            }

            // 3. Exit conditions
            if (_slideTimer >= _config.MaxSlideDuration)
            {
                ExitSlide();
                return;
            }
            if (currentSlideSpeed < _config.MinSlideExitSpeed)
            {
                ExitSlide();
                return;
            }

            // 4. Calculate slide velocity
            Vector3 slideVelocity = _slideDirection * currentSlideSpeed;

            // 5. Apply player steering
            if (_input.MoveInput.magnitude > 0.1f)
            {
                Vector3 steerInput =
                    (
                        _motor.CharacterRight * _input.MoveInput.x
                        + _motor.CharacterForward * _input.MoveInput.y
                    ) * _config.SlideSteerStrength;
                Vector3 lateralSteer = Vector3.Project(
                    steerInput,
                    Vector3.Cross(_motor.CharacterUp, _slideDirection)
                );
                slideVelocity += lateralSteer * deltaTime;

                // Update slide direction to match new velocity
                if (slideVelocity.magnitude > 0.1f)
                    _slideDirection = slideVelocity.normalized;
            }

            // 6. Project to ground plane and set final velocity
            if (_motor.GroundingStatus.FoundAnyGround)
            {
                slideVelocity =
                    _motor.GetDirectionTangentToSurface(
                        slideVelocity,
                        _motor.GroundingStatus.GroundNormal
                    ) * slideVelocity.magnitude;
            }

            currentVelocity = slideVelocity;
        }
    }
}
```
