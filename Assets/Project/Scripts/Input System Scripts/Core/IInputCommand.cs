using System;
using UnityEngine.InputSystem;

/// <summary>
/// Simple command interface for routing Unity Input System callbacks to game logic.
/// </summary>
public interface IInputCommand
{
    /// <summary>
    /// Execute in response to an input callback.
    /// Use context.phase to branch on Started/Performed/Canceled and ReadValue<T>() for values.
    /// </summary>
    /// <param name="context">Unity Input System callback context.</param>
    void Execute(InputAction.CallbackContext context);
}
