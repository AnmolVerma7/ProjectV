using System;
using UnityEngine.InputSystem;

/// <summary>
/// A command for button-like inputs. Exposes separate delegates for each phase.
/// </summary>
public sealed class ButtonCommand : IInputCommand
{
    private readonly Action _onStarted;
    private readonly Action _onPerformed;
    private readonly Action _onCanceled;

    /// <summary>
    /// Creates a simple button command with only a Performed callback.
    /// </summary>
    public ButtonCommand(Action onPerformed)
        : this(null, onPerformed, null) { }

    /// <summary>
    /// Creates a button command with callbacks for all phases.
    /// </summary>
    public ButtonCommand(Action onStarted, Action onPerformed, Action onCanceled)
    {
        _onStarted = onStarted;
        _onPerformed = onPerformed;
        _onCanceled = onCanceled;
    }

    public void Execute(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                _onStarted?.Invoke();
                break;
            case InputActionPhase.Performed:
                _onPerformed?.Invoke();
                break;
            case InputActionPhase.Canceled:
                _onCanceled?.Invoke();
                break;
        }
    }
}
