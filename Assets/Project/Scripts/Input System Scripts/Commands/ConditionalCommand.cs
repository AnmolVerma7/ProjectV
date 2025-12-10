using System;
using UnityEngine.InputSystem;

/// <summary>
/// Routes execution to one of two commands based on a predicate.
/// Useful for contextual overrides (e.g., HackMenu held -> different behavior).
/// </summary>
public sealed class ConditionalCommand : IInputCommand
{
    private readonly Func<bool> _predicate;
    private readonly IInputCommand _whenTrue;
    private readonly IInputCommand _whenFalse;

    /// <summary>
    /// Creates a conditional command.
    /// </summary>
    /// <param name="predicate">The condition to check.</param>
    /// <param name="whenTrue">Command to execute if true.</param>
    /// <param name="whenFalse">Command to execute if false.</param>
    public ConditionalCommand(Func<bool> predicate, IInputCommand whenTrue, IInputCommand whenFalse)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        _whenTrue = whenTrue ?? throw new ArgumentNullException(nameof(whenTrue));
        _whenFalse = whenFalse ?? throw new ArgumentNullException(nameof(whenFalse));
    }

    public void Execute(InputAction.CallbackContext context)
    {
        if (_predicate())
            _whenTrue.Execute(context);
        else
            _whenFalse.Execute(context);
    }
}
