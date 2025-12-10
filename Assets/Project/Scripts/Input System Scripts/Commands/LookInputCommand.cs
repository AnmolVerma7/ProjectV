using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Routes Look input differently based on device:
/// - Pointer/Mouse: treat value as per-frame delta (accumulate)
/// - Gamepad/Joystick: treat value as a continuous rate (set), zero on cancel
/// </summary>
public sealed class LookInputCommand : IInputCommand
{
    private readonly Action<Vector2> _addDelta;
    private readonly Action<Vector2> _setRate;

    /// <summary>
    /// Creates a new look input command.
    /// </summary>
    /// <param name="addDelta">Callback for delta-based input (mouse).</param>
    /// <param name="setRate">Callback for rate-based input (gamepad).</param>
    public LookInputCommand(Action<Vector2> addDelta, Action<Vector2> setRate)
    {
        _addDelta = addDelta;
        _setRate = setRate;
    }

    public void Execute(InputAction.CallbackContext context)
    {
        if (_addDelta == null || _setRate == null)
            return;

        switch (context.phase)
        {
            case InputActionPhase.Performed:
            case InputActionPhase.Started:
            {
                var v = context.ReadValue<Vector2>();
                var device = context.control?.device;
                if (device is Mouse || device is Pointer || device is Pen)
                {
                    _addDelta(v);
                }
                else if (device is Gamepad || device is Joystick)
                {
                    _setRate(v);
                }
                else
                {
                    // Fallback: treat as rate for stability on unknown devices
                    _setRate(v);
                }
                break;
            }
            case InputActionPhase.Canceled:
            {
                var device = context.control?.device;
                if (device is Gamepad || device is Joystick)
                {
                    _setRate(Vector2.zero);
                }
                else if (device is Mouse || device is Pointer || device is Pen)
                {
                    // No-op; deltas are frame-accumulated and reset by camera
                }
                break;
            }
        }
    }
}
