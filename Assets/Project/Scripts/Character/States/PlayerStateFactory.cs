using Antigravity.Controllers;

namespace Antigravity.Character.States
{
    /// <summary>
    /// Factory for creating and caching player state instances.
    /// <para>
    /// <strong>Performance:</strong> States are created once and cached to prevent GC allocations
    /// on every state transition. The ?? operator ensures lazy initialization.
    /// </para>
    /// </summary>
    public class PlayerStateFactory
    {
        private PlayerStateMachine _context;
        private PlayerController _controller;

        // Cached state instances
        private PlayerBaseState _grounded;
        private PlayerBaseState _air;
        private PlayerBaseState _idle;
        private PlayerBaseState _move;
        private PlayerBaseState _jump;
        private PlayerBaseState _doubleJump;
        private PlayerBaseState _fall;

        /// <summary>
        /// Constructs the factory with references needed to create states.
        /// </summary>
        public PlayerStateFactory(
            PlayerStateMachine currentContext,
            PlayerController playerController
        )
        {
            _context = currentContext;
            _controller = playerController;
        }

        /// <summary>Gets or creates the Grounded state (root state when on ground).</summary>
        public PlayerBaseState Grounded() =>
            _grounded ??= new PlayerGroundedState(_context, this, _controller);

        /// <summary>Gets or creates the Air state (root state when airborne).</summary>
        public PlayerBaseState Air() => _air ??= new PlayerAirState(_context, this, _controller);

        /// <summary>Gets or creates the Idle state (substate of Grounded when not moving).</summary>
        public PlayerBaseState Idle() => _idle ??= new PlayerIdleState(_context, this, _controller);

        /// <summary>Gets or creates the Move state (substate of Grounded when moving).</summary>
        public PlayerBaseState Move() => _move ??= new PlayerMoveState(_context, this, _controller);

        /// <summary>Gets or creates the Jump state (substate of Air when ascending from first jump).</summary>
        public PlayerBaseState Jump() => _jump ??= new PlayerJumpState(_context, this, _controller);

        /// <summary>Gets or creates the DoubleJump state (substate of Air when ascending from second jump).</summary>
        public PlayerBaseState DoubleJump() =>
            _doubleJump ??= new PlayerDoubleJumpState(_context, this, _controller);

        /// <summary>Gets or creates the Fall state (substate of Air when descending).</summary>
        public PlayerBaseState Fall() => _fall ??= new PlayerFallState(_context, this, _controller);
    }
}
