using Antigravity.Controllers;
using UnityEngine;

namespace Antigravity.Movement
{
    /// <summary>
    /// Encapsulates all dash-related logic and state.
    /// Handles charge management, cooldown, and impulse application.
    /// </summary>
    public class DashHandler
    {
        private readonly PlayerMovementConfig _config;

        // State
        private int _currentDashCharges;
        private float _dashReloadTimer;
        private float _dashAuthenticationTimer; // "Intermission" timer (prevents spam)
        private bool _pendingDash;

        /// <summary>
        /// Current number of dash charges available.
        /// </summary>
        public int CurrentDashCharges => _currentDashCharges;

        public DashHandler(PlayerMovementConfig config)
        {
            _config = config;
            // Initialize with full charges
            _currentDashCharges = config.MaxDashCharges;
        }

        /// <summary>
        /// Resets dash state when module is activated.
        /// </summary>
        public void OnActivated()
        {
            _pendingDash = false;
            _dashAuthenticationTimer = 0f;
        }

        /// <summary>
        /// Called by PlayerController when dash button is pressed.
        /// </summary>
        public void RequestDash()
        {
            _pendingDash = true;
        }

        /// <summary>
        /// Updates charge reload and intermission timers.
        /// Called from AfterUpdate.
        /// </summary>
        public void UpdateCharges(float deltaTime)
        {
            if (_dashAuthenticationTimer > 0)
                _dashAuthenticationTimer -= deltaTime;

            if (_currentDashCharges < _config.MaxDashCharges)
            {
                _dashReloadTimer += deltaTime;
                if (_dashReloadTimer >= _config.DashReloadTime)
                {
                    _currentDashCharges++;
                    _dashReloadTimer = 0f;
                }
            }
            else
            {
                _dashReloadTimer = 0f;
            }
        }

        /// <summary>
        /// Attempts to apply dash impulse if conditions are met.
        /// Returns true if dash was applied.
        /// </summary>
        public bool TryApplyDash(ref Vector3 velocityAdd, Vector3 direction)
        {
            if (
                _pendingDash
                && _dashAuthenticationTimer <= 0
                && _currentDashCharges > 0
                && direction.sqrMagnitude > 0
            )
            {
                velocityAdd += direction * _config.DashForce;
                _currentDashCharges--;
                _dashAuthenticationTimer = _config.DashIntermissionTime;
                _pendingDash = false;
                return true;
            }

            _pendingDash = false;
            return false;
        }
    }
}
