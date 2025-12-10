using Antigravity.Controllers;
using UnityEngine;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Root state when the player is on the ground.
    /// <para>
    /// <strong>Substates:</strong> Idle (standing still) or Move (moving).
    /// </para>
    /// <para>
    /// <strong>Transitions:</strong> Switches to Air state when leaving the ground.
    /// </para>
    /// </summary>
    public class PlayerGroundedState : PlayerBaseState
    {
        public PlayerGroundedState(
            PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory,
            PlayerController playerController
        )
            : base(currentContext, playerStateFactory, playerController) { }

        public override void EnterState()
        {
            // Initialize substates when entering grounded (determines Idle vs Move)
            InitializeSubState();
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
            // Transition to Air if we leave the ground
            if (!Controller.Motor.GroundingStatus.IsStableOnGround)
            {
                SwitchState(Factory.Air());
            }
        }

        public override void InitializeSubState()
        {
            // Determine substate based on movement input
            if (Controller.InputHandler.MoveInput.sqrMagnitude > 0.01f)
            {
                SetSubState(Factory.Move());
            }
            else
            {
                SetSubState(Factory.Idle());
            }
        }
    }
}
