using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Executes multiple IInputCommand instances in order for the same callback context.
/// Useful for layering behaviors (e.g., logging + combo recognition + gameplay trigger).
/// </summary>
public sealed class CompositeCommand : IInputCommand
{
    private readonly List<IInputCommand> _commands = new();

    /// <summary>
    /// Creates a composite command from a list of sub-commands.
    /// </summary>
    public CompositeCommand(params IInputCommand[] commands)
    {
        if (commands != null)
            _commands.AddRange(commands);
    }

    /// <summary>
    /// Add another command to the sequence.
    /// </summary>
    public void Add(IInputCommand command)
    {
        if (command != null)
            _commands.Add(command);
    }

    public void Execute(InputAction.CallbackContext context)
    {
        for (int i = 0; i < _commands.Count; i++)
        {
            _commands[i].Execute(context);
        }
    }
}
