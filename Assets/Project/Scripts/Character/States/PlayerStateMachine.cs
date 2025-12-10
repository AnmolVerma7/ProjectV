using Antigravity.Controllers;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Core state machine that manages player state transitions.
    /// This machine tracks the current state and provides a method to change states.
    /// <para>
    /// <strong>Design:</strong> The state machine itself is simple - it just holds the current state.
    /// All logic is delegated to the states themselves via the State Pattern.
    /// </para>
    /// </summary>
    public class PlayerStateMachine
    {
        /// <summary>
        /// The currently active state. States can read this to check what's active.
        /// Setting is private - use ChangeState() to transition.
        /// </summary>
        public PlayerBaseState CurrentState { get; private set; }

        /// <summary>
        /// Initializes the state machine with a starting state.
        /// Call this once during PlayerController.Awake().
        /// </summary>
        /// <param name="startingState">The initial state to enter (typically Grounded)</param>
        public void Initialize(PlayerBaseState startingState)
        {
            CurrentState = startingState;
            CurrentState.EnterState();
        }

        /// <summary>
        /// Transitions to a new state, calling Exit on the old state and Enter on the new.
        /// This is the ONLY way states should change the root state.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        public void ChangeState(PlayerBaseState newState)
        {
            CurrentState.ExitState();
            CurrentState = newState;
            CurrentState.EnterState();
        }
    }
}
