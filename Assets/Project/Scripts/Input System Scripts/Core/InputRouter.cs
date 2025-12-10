using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [PROJECT SPECIFIC] - BRIDGE
/// This script binds the Generic Input System to your Unity-Generated Input Class (e.g. "InputMap").
/// <para>
/// STEPS TO REUSE:
/// 1. Rename "InputMap" references to your generated class name.
/// 2. Implement the specific Interface from that class (e.g. InputMap.IPlayerActions).
/// 3. Update the Dispatch calls to match your actions.
/// </para>
/// </summary>
[DefaultExecutionOrder(-200)]
public class InputRouter : MonoBehaviour, InputMap.IPlayerActions
{
    [SerializeField]
    private bool _autoEnable = true;

    [SerializeField]
    private bool _debugMissingBindings = false;

    private InputMap _input;
    private readonly Dictionary<InputAction, IInputCommand> _commands = new();

    /// <summary>
    /// Access to the underlying generated InputMap.
    /// </summary>
    public InputMap Input => _input;

    /// <summary>
    /// Access to the Player action map.
    /// </summary>
    public InputMap.PlayerActions Player => _input.Player;

    private void Awake()
    {
        _input = new InputMap();
        _input.Player.AddCallbacks(this);
    }

    private void OnEnable()
    {
        if (_input != null && _autoEnable)
            _input.Player.Enable();
    }

    private void OnDisable()
    {
        if (_input != null)
            _input.Player.Disable();
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.Player.RemoveCallbacks(this);
            _input.Dispose();
        }
    }

    /// <summary>
    /// Bind an input action to a command. Replaces any prior binding for the action.
    /// </summary>
    public void Bind(InputAction action, IInputCommand command)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        _commands[action] = command;
    }

    /// <summary>
    /// Convenience overload: find action by name on the underlying InputMap and bind it.
    /// </summary>
    public void Bind(string actionName, IInputCommand command)
    {
        if (string.IsNullOrEmpty(actionName))
            throw new ArgumentException("Action name is null or empty", nameof(actionName));
        if (_input == null)
            throw new InvalidOperationException("InputMap not initialized yet");
        var action = _input.FindAction(actionName, throwIfNotFound: true);
        Bind(action, command);
    }

    /// <summary>
    /// Remove a command binding for the given action.
    /// </summary>
    public void Unbind(InputAction action)
    {
        if (action == null)
            return;
        _commands.Remove(action);
    }

    /// <summary>
    /// Remove all command bindings.
    /// </summary>
    public void ClearBindings() => _commands.Clear();

    /// <summary>
    /// Try get a command bound to an action.
    /// </summary>
    public bool TryGetCommand(InputAction action, out IInputCommand command) =>
        _commands.TryGetValue(action, out command);

    private void Dispatch(InputAction.CallbackContext context)
    {
        if (context.action != null && _commands.TryGetValue(context.action, out var command))
        {
            command.Execute(context);
        }
        else if (_debugMissingBindings && context.action != null)
        {
            Debug.LogWarning(
                $"InputRouter: No command bound for action '{context.action.name}'",
                this
            );
        }
    }

    // IPlayerActions implementation: all simply dispatch using the context.
    public void OnOptions(InputAction.CallbackContext context) => Dispatch(context);

    public void OnPause(InputAction.CallbackContext context) => Dispatch(context);

    public void OnMove(InputAction.CallbackContext context) => Dispatch(context);

    public void OnLook(InputAction.CallbackContext context) => Dispatch(context);

    public void OnJump(InputAction.CallbackContext context) => Dispatch(context);

    public void OnSprint(InputAction.CallbackContext context) => Dispatch(context);

    public void OnCrouchSlideCancel(InputAction.CallbackContext context) => Dispatch(context);

    public void OnDash(InputAction.CallbackContext context) => Dispatch(context);

    public void OnAttackOrInteract(InputAction.CallbackContext context) => Dispatch(context);

    public void OnTeleportAttack(InputAction.CallbackContext context) => Dispatch(context);

    public void OnAim(InputAction.CallbackContext context) => Dispatch(context);

    public void OnThrow(InputAction.CallbackContext context) => Dispatch(context);

    public void OnLockOnToggle(InputAction.CallbackContext context) => Dispatch(context);

    public void OnAbilityMenu(InputAction.CallbackContext context) => Dispatch(context);

    public void OnHackMenu(InputAction.CallbackContext context) => Dispatch(context);

    public void OnActivateHackAbility(InputAction.CallbackContext context) => Dispatch(context);

    public void OnHeal(InputAction.CallbackContext context) => Dispatch(context);

    public void OnCrouch(InputAction.CallbackContext context) => Dispatch(context);

    public void OnShoot(InputAction.CallbackContext context) => Dispatch(context);
}
