using Antigravity.Controllers;
using UnityEngine;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Substate of Air during the upward phase of the second jump (double jump).
    /// <para>
    /// <strong>Observation Only:</strong> PlayerController.UpdateVelocity() handles jump physics.
    /// This state just tracks that we're in the "double jumping" phase.
    /// </para>
    /// <para>
    /// <strong>Grace Period:</strong> Waits 0.05s before checking for fall to prevent instant transition.
    /// Without this, the state would immediately switch to Fall before physics applies the jump force.
    /// </para>
    /// <para>
    /// <strong>Transitions:</strong> To Fall when reaching apex (velocity becomes negative after grace period).
    /// </para>
    /// </summary>
    public class PlayerDoubleJumpState : PlayerBaseState
    {
        private float _timeInState;
        private const float GRACE_PERIOD = 0.05f; // 50ms grace to let physics apply

        public PlayerDoubleJumpState(
            PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory,
            PlayerController playerController
        )
            : base(currentContext, playerStateFactory, playerController) { }

        public override void EnterState()
        {
            _timeInState = 0f;
            // Could trigger double jump animation here in the future
        }

        public override void UpdateState()
        {
            _timeInState += UnityEngine.Time.deltaTime;
            CheckSwitchStates();
        }

        public override void ExitState()
        {
            // No cleanup needed
        }

        public override void CheckSwitchStates()
        {
            // Wait for grace period before checking velocity
            // This prevents instant fall before physics engine applies the jump force
            if (_timeInState > GRACE_PERIOD && Controller.Motor.BaseVelocity.y < 0f)
            {
                SwitchState(Factory.Fall());
            }
        }

        public override void InitializeSubState()
        {
            // DoubleJump is a leaf state - no substates
        }
    }
}
