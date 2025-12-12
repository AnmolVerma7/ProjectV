```csharp
using Antigravity.Controllers;
using KinematicCharacterController;
using UnityEngine;

namespace Antigravity.Movement
{
    /// <summary>
    /// Debug movement module for flying through geometry.
    /// Handles collision disabling and simple directional movement.
    /// </summary>
    public class NoClipMovement : MovementModuleBase
    {
        private readonly PlayerInputHandler _input;
        private Vector3 _moveInputVector;

        public NoClipMovement(
            KinematicCharacterMotor motor,
            PlayerMovementConfig config,
            PlayerInputHandler input
        )
            : base(motor, config)
        {
            _input = input;
        }

        public override void OnActivated()
        {
            // Disable physics interactions
            Motor.SetCapsuleCollisionsActivation(false);
            Motor.SetMovementCollisionsSolvingActivation(false);
            Motor.SetGroundSolvingActivation(false);
        }

        public override void OnDeactivated()
        {
            // Re-enable physics interactions
            Motor.SetCapsuleCollisionsActivation(true);
            Motor.SetMovementCollisionsSolvingActivation(true);
            Motor.SetGroundSolvingActivation(true);
        }

        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_moveInputVector != Vector3.zero && Config.OrientationSharpness > 0f)
            {
                Vector3 smoothedLookInputDirection = Vector3
                    .Slerp(
                        Motor.CharacterForward,
                        _moveInputVector,
                        1 - Mathf.Exp(-Config.OrientationSharpness * deltaTime)
                    )
                    .normalized;

                currentRotation = Quaternion.LookRotation(
                    smoothedLookInputDirection,
                    Motor.CharacterUp
                );
            }

            if (Config.OrientTowardsGravity)
            {
                currentRotation =
                    Quaternion.FromToRotation((currentRotation * Vector3.up), -Config.Gravity)
                    * currentRotation;
            }
        }

        public override void UpdatePhysics(ref Vector3 currentVelocity, float deltaTime)
        {
            // Vertical input (Space/Crouch)
            float verticalInput = (_input.JumpHeld ? 1f : 0f) + (_input.CrouchHeld ? -1f : 0f);

            // Calculate target velocity (planar move + vertical)
            Vector3 targetVelocity =
                (_moveInputVector + (Motor.CharacterUp * verticalInput)).normalized
                * Config.NoClipMoveSpeed;

            // Smooth movement
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                1 - Mathf.Exp(-Config.NoClipSharpness * deltaTime)
            );
        }

        public override void AfterUpdate(float deltaTime)
        {
            // No cleanup needed for NoClip
        }

        /// <summary>
        /// Updates the move direction (called from Controller)
        /// </summary>
        public void SetMoveInput(Vector3 input)
        {
            _moveInputVector = input;
        }
    }
}
```
