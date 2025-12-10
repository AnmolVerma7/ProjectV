using Antigravity.Controllers;
using UnityEngine;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Root state when the player is airborne (not on ground).
    /// <para>
    /// <strong>Substates:</strong> Jump (ascending), Fall (descending), or DoubleJump (second jump).
    /// </para>
    /// <para>
    /// <strong>Transitions:</strong> Switches to Grounded state when landing.
    /// </para>
    /// </summary>
    public class PlayerAirState : PlayerBaseState
    {
        public PlayerAirState(
            PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory,
            PlayerController playerController
        )
            : base(currentContext, playerStateFactory, playerController) { }

        public override void EnterState()
        {
            // Re-initialize substate every time we enter Air (determines Jump vs Fall)
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
            // Transition to Grounded when landing
            if (Controller.Motor.GroundingStatus.IsStableOnGround)
            {
                SwitchState(Factory.Grounded());
            }
        }

        public override void InitializeSubState()
        {
            // Determine initial air substate based on vertical velocity
            // Positive velocity = ascending (Jump), negative/zero = descending (Fall)
            if (Controller.Motor.BaseVelocity.y > 0f)
            {
                SetSubState(Factory.Jump());
            }
            else
            {
                SetSubState(Factory.Fall());
            }
        }
    }
}
