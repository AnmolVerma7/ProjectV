using Antigravity.Controllers;
using UnityEngine;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Substate of Grounded when the player is standing still.
    /// <para>
    /// <strong>Transitions:</strong> Switches to Move state when movement input is detected.
    /// </para>
    /// </summary>
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(
            PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory,
            PlayerController playerController
        )
            : base(currentContext, playerStateFactory, playerController) { }

        public override void EnterState()
        {
            // Could trigger idle animation here in the future
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
            // Switch to Move if player starts moving
            if (Controller.InputHandler.MoveInput.sqrMagnitude > 0.01f)
            {
                SwitchState(Factory.Move());
            }
        }

        public override void InitializeSubState()
        {
            // Idle is a leaf state - no substates
        }
    }
}
