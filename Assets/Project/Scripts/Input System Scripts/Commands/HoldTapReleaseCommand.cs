using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Decides tap vs hold on release using a configurable threshold.
/// Optional onPressed/onReleased for additional hooks.
/// </summary>
public sealed class HoldTapReleaseCommand : IInputCommand
{
    private readonly float _holdThresholdSeconds;
    private readonly Action _onTap;
    private readonly Action _onHold;
    private readonly Action _onPressed;
    private readonly Action _onReleased;
    private readonly Action<string> _onExternallyCanceled;

    private float _startedAt;
    private bool _started;

    /// <summary>
    /// Creates a new Hold/Tap command.
    /// </summary>
    /// <param name="holdThresholdSeconds">Duration to distinguish hold from tap.</param>
    /// <param name="onTap">Fired if released before threshold.</param>
    /// <param name="onHold">Fired if released after threshold.</param>
    /// <param name="onPressed">Fired immediately on press.</param>
    /// <param name="onReleased">Fired immediately on release (always).</param>
    /// <param name="onExternallyCanceled">Fired if canceled by another system.</param>
    public HoldTapReleaseCommand(
        float holdThresholdSeconds,
        Action onTap,
        Action onHold,
        Action onPressed = null,
        Action onReleased = null,
        Action<string> onExternallyCanceled = null
    )
    {
        _holdThresholdSeconds = Mathf.Max(0f, holdThresholdSeconds);
        _onTap = onTap;
        _onHold = onHold;
        _onPressed = onPressed;
        _onReleased = onReleased;
        _onExternallyCanceled = onExternallyCanceled;
    }

    public void Execute(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                _started = true;
                _startedAt = Time.time;
                _onPressed?.Invoke();
                break;
            case InputActionPhase.Performed:
                // Intentionally no-op; final decision happens on Canceled.
                break;
            case InputActionPhase.Canceled:
                if (_started)
                {
                    var duration = Time.time - _startedAt;
                    if (duration >= _holdThresholdSeconds)
                        _onHold?.Invoke();
                    else
                        _onTap?.Invoke();

                    _onReleased?.Invoke();
                    _started = false;
                }
                break;
        }
    }

    /// <summary>
    /// Cancel the ongoing press from an external event (e.g., another button like C/S/C).
    /// Emits the external cancel callback and a Release, without invoking Tap/Hold.
    /// </summary>
    public void Cancel(string reason = null)
    {
        if (!_started)
            return;
        _onExternallyCanceled?.Invoke(reason);
        _onReleased?.Invoke();
        _started = false;
    }

    /// <summary>
    /// True while the button is currently pressed (between Started and Canceled).
    /// </summary>
    public bool IsPressed => _started;
}
