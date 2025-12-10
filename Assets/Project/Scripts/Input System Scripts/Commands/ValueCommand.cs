using System;
using UnityEngine.InputSystem;

/// <summary>
/// A command for value-type inputs (e.g., Vector2 for Move/Look).
/// </summary>
public sealed class ValueCommand<T> : IInputCommand
    where T : struct
{
    private readonly Action<T> _onPerformed;
    private readonly Action<T> _onStarted;
    private readonly Action _onCanceled;
    private readonly Func<T> _canceledValueProvider;

    /// <summary>
    /// Create a value command that invokes onPerformed with the current value when performed.
    /// </summary>
    public ValueCommand(Action<T> onPerformed)
        : this(null, onPerformed, null, null) { }

    /// <summary>
    /// Full constructor for separate phase handlers and optional canceled value provider.
    /// </summary>
    public ValueCommand(
        Action<T> onStarted,
        Action<T> onPerformed,
        Action onCanceled,
        Func<T> canceledValueProvider
    )
    {
        _onStarted = onStarted;
        _onPerformed = onPerformed;
        _onCanceled = onCanceled;
        _canceledValueProvider = canceledValueProvider;
    }

    public void Execute(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                if (_onStarted != null)
                {
                    var vS = context.ReadValue<T>();
                    _onStarted.Invoke(vS);
                }
                break;
            case InputActionPhase.Performed:
                if (_onPerformed != null)
                {
                    var vP = context.ReadValue<T>();
                    _onPerformed.Invoke(vP);
                }
                break;
            case InputActionPhase.Canceled:
                _onCanceled?.Invoke();
                if (_canceledValueProvider != null && _onPerformed != null)
                {
                    // Emit a final value on cancel to let receivers reset state if desired.
                    _onPerformed.Invoke(_canceledValueProvider());
                }
                break;
        }
    }
}
