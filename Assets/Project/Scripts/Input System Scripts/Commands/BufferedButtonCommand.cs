using System;
using UnityEngine.InputSystem;

/// <summary>
/// Button command that buffers Performed when a gate is false, for a short window.
/// Started/Canceled pass through immediately.
/// </summary>
public sealed class BufferedButtonCommand : IInputCommand
{
    private readonly Func<bool> _gate;
    private readonly float _bufferWindowSeconds;
    private readonly Action _onStarted;
    private readonly Action _onPerformed;
    private readonly Action _onCanceled;

    /// <summary>
    /// Creates a new buffered button command.
    /// </summary>
    /// <param name="gate">Condition that must be true to fire immediately.</param>
    /// <param name="bufferWindowSeconds">Time window to keep trying if gate is false.</param>
    /// <param name="onStarted">Callback for Started phase.</param>
    /// <param name="onPerformed">Callback for Performed phase (buffered if gate is false).</param>
    /// <param name="onCanceled">Callback for Canceled phase.</param>
    public BufferedButtonCommand(
        Func<bool> gate,
        float bufferWindowSeconds,
        Action onStarted,
        Action onPerformed,
        Action onCanceled
    )
    {
        _gate = gate ?? (() => true);
        _bufferWindowSeconds = bufferWindowSeconds;
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
                if (_gate())
                {
                    _onPerformed?.Invoke();
                }
                else
                {
                    InputBufferService.Instance?.Buffer(_onPerformed, _gate, _bufferWindowSeconds);
                }
                break;
            case InputActionPhase.Canceled:
                _onCanceled?.Invoke();
                break;
        }
    }
}
