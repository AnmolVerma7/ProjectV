using Antigravity.Controllers;
using UnityEngine;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Substate of Air when the player is descending (falling).
    /// <para>
    /// <strong>Observation Only:</strong> PlayerController.UpdateVelocity() handles gravity and physics.
    /// This state just tracks that we're in the "falling" phase.
    /// </para>
    /// <para>
    /// <strong>Transitions:</strong> To DoubleJump if velocity becomes positive (player executed double jump).
    /// </para>
    /// </summary>
    public class PlayerFallState : PlayerBaseState
    {
        private const float UPWARD_VELOCITY_THRESHOLD = 0.5f; // Velocity threshold to detect double jump

        public PlayerFallState(
            PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory,
            PlayerController playerController
        )
            : base(currentContext, playerStateFactory, playerController) { }

        public override void EnterState()
        {
            // Could trigger fall animation here in the future
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void ExitState()
        {
            // No cleanup needed
        }

        public override void CheckSwitchStates()
        {
            // If velocity becomes positive while falling, player executed a double jump
            // (PlayerController applied the jump force in UpdateVelocity)
            if (Controller.Motor.BaseVelocity.y > UPWARD_VELOCITY_THRESHOLD)
            {
                SwitchState(Factory.DoubleJump());
            }
        }

        public override void InitializeSubState()
        {
            // Fall is a leaf state - no substates
        }
    }
}
