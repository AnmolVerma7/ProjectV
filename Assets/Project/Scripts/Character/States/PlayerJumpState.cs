using Antigravity.Controllers;
using UnityEngine;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Substate of Air during the upward phase of the first jump.
    /// <para>
    /// <strong>Observation Only:</strong> PlayerController.UpdateVelocity() handles jump physics.
    /// This state just tracks that we're in the "jumping" phase.
    /// </para>
    /// <para>
    /// <strong>Transitions:</strong>
    /// - To DoubleJump if jump input detected while ascending
    /// - To Fall when reaching apex (velocity becomes negative)
    /// </para>
    /// </summary>
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(
            PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory,
            PlayerController playerController
        )
            : base(currentContext, playerStateFactory, playerController) { }

        public override void EnterState()
        {
            // Could trigger jump animation here in the future
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
            // If player presses jump again while ascending, transition to DoubleJump
            if (Controller.InputHandler.JumpDown)
            {
                SwitchState(Factory.DoubleJump());
            }
            // Transition to Fall when vertical velocity becomes negative (apex of jump)
            else if (Controller.Motor.BaseVelocity.y < 0f)
            {
                SwitchState(Factory.Fall());
            }
        }

        public override void InitializeSubState()
        {
            // Jump is a leaf state - no substates
        }
    }
}
