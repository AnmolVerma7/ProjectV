using System;
using UnityEngine.InputSystem;

/// <summary>
/// Toggles a setting or state when the button/action is Performed.
/// Useful for menu toggles (sprint mode, crouch mode) or debug flags.
/// </summary>
public sealed class ToggleCommand : IInputCommand
{
    private readonly Action _toggle;

    /// <summary>
    /// Creates a new toggle command.
    /// </summary>
    /// <param name="toggle">Action to invoke on toggle.</param>
    public ToggleCommand(Action toggle)
    {
        _toggle = toggle;
    }

    public void Execute(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            _toggle?.Invoke();
        }
    }
}
