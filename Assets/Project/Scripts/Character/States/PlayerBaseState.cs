using Antigravity.Controllers;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Abstract base class for all player states in the Hierarchical State Machine.
    /// <para>
    /// <strong>Observation Only:</strong> States observe PlayerController but do NOT control physics.
    /// Physics (jumps, movement) are handled in PlayerController.UpdateVelocity().
    /// </para>
    /// <para>
    /// <strong>Hierarchy Support:</strong> States can have parent (super) and child (sub) states.
    /// Example: Air (super) > Jump (sub) means Jump is a child state of Air.
    /// </para>
    /// </summary>
    public abstract class PlayerBaseState
    {
        /// <summary>Context reference to the state machine managing this state.</summary>
        protected PlayerStateMachine Context { get; private set; }

        /// <summary>Factory for creating other state instances.</summary>
        protected PlayerStateFactory Factory { get; private set; }

        /// <summary>Reference to the player controller being observed.</summary>
        protected PlayerController Controller { get; private set; }

        private PlayerBaseState _currentSuperState;
        private PlayerBaseState _currentSubState;

        /// <summary>Gets the parent state if this is a substate (e.g., Jump's parent is Air).</summary>
        public PlayerBaseState CurrentSuperState => _currentSuperState;

        /// <summary>Gets the active child state if this state has substates. Used for debug display.</summary>
        public PlayerBaseState CurrentSubState => _currentSubState;

        /// <summary>
        /// Constructor called by state factory. Stores references needed by all states.
        /// </summary>
        public PlayerBaseState(
            PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory,
            PlayerController playerController
        )
        {
            Context = currentContext;
            Factory = playerStateFactory;
            Controller = playerController;
        }

        #region Abstract Methods (Must be implemented by concrete states)

        /// <summary>Called once when entering this state. Setup logic goes here.</summary>
        public abstract void EnterState();

        /// <summary>Called every frame while this state is active. Update logic goes here.</summary>
        public abstract void UpdateState();

        /// <summary>Called once when exiting this state. Cleanup logic goes here.</summary>
        public abstract void ExitState();

        /// <summary>
        /// Checks conditions for transitioning to other states.
        /// Called from UpdateState(). Use SwitchState() to transition.
        /// </summary>
        public abstract void CheckSwitchStates();

        /// <summary>
        /// Initializes substates for hierarchical states (e.g., Grounded initializes Idle/Move).
        /// Leave empty for leaf states (states without children).
        /// </summary>
        public abstract void InitializeSubState();

        #endregion

        #region State Update Chain

        /// <summary>
        /// Updates this state and all substates recursively.
        /// Called from PlayerController.Update() on the root state only.
        /// </summary>
        public void UpdateStates()
        {
            UpdateState();
            if (_currentSubState != null)
            {
                _currentSubState.UpdateStates();
            }
        }

        #endregion

        #region State Transition Helpers

        /// <summary>
        /// Transitions to a new state at the same level in the hierarchy.
        /// For substates, updates the parent's substate. For root states, updates the state machine.
        /// </summary>
        /// <param name="newState">The state to switch to</param>
        protected void SwitchState(PlayerBaseState newState)
        {
            ExitState();
            newState.EnterState();

            if (_currentSuperState != null)
            {
                // We're a substate - tell our parent about the new substate
                _currentSuperState.SetSubState(newState);
            }
            else
            {
                // We're a root state - tell the state machine
                Context.ChangeState(newState);
            }
        }

        /// <summary>Sets this state's parent state. Called automatically by SetSubState().</summary>
        protected void SetSuperState(PlayerBaseState newSuperState)
        {
            _currentSuperState = newSuperState;
        }

        /// <summary>
        /// Sets this state's child state and establishes the parent-child relationship.
        /// </summary>
        /// <param name="newSubState">The child state to set</param>
        protected void SetSubState(PlayerBaseState newSubState)
        {
            _currentSubState = newSubState;
            newSubState.SetSuperState(this);
        }

        #endregion
    }
}
