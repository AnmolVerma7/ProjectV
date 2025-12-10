using Antigravity.Controllers;
using UnityEngine;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Substate of Grounded when the player is moving.
    /// <para>
    /// <strong>Transitions:</strong> Switches to Idle state when movement stops.
    /// </para>
    /// </summary>
    public class PlayerMoveState : PlayerBaseState
    {
        public PlayerMoveState(
            PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory,
            PlayerController playerController
        )
            : base(currentContext, playerStateFactory, playerController) { }

        public override void EnterState()
        {
            // Could trigger run/walk animation here in the future
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
            // Switch to Idle if player stops moving
            if (Controller.InputHandler.MoveInput.sqrMagnitude <= 0.01f)
            {
                SwitchState(Factory.Idle());
            }
        }

        public override void InitializeSubState()
        {
            // Move is a leaf state - no substates
        }
    }
}
