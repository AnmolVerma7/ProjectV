/// <summary>
/// [PROJECT SPECIFIC] - TEMPLATE
/// This class defines the shared input state for YOUR specific game.
/// <para>
/// You should granularly add flags here that different systems need to read/write.
/// Example: "HackMenuHeld" is specific to this game. A racing game might have "NitroHeld".
/// </para>
/// </summary>
public interface IGameInputContext
{
    /// <summary>True while Hack Menu button is physically held.</summary>
    bool HackMenuHeld { get; set; }

    /// <summary>True while Ability Menu button is physically held.</summary>
    bool AbilityMenuHeld { get; set; }

    /// <summary>True if a valid target for hacking exists (set by gameplay systems).</summary>
    bool HasValidHackTarget { get; set; }

    /// <summary>When false, consume taps are buffered until it becomes true.</summary>
    bool AbilityAcceptsInput { get; set; }

    /// <summary>
    /// During a single Hack Menu hold, the C/S/C-to-exit override should only fire once.
    /// After it triggers, set this to true to let subsequent C/S/C presses behave normally
    /// (e.g., crouch). Reset on next Hack Menu press.
    /// </summary>
    bool HackMenuOverrideConsumed { get; set; }
}

/// <summary>
/// Standard implementation of <see cref="IGameInputContext"/>.
/// </summary>
public sealed class GameInputContext : IGameInputContext
{
    /// <inheritdoc />
    public bool HackMenuHeld { get; set; }

    /// <inheritdoc />
    public bool AbilityMenuHeld { get; set; }

    /// <inheritdoc />
    public bool HasValidHackTarget { get; set; }

    /// <inheritdoc />
    public bool AbilityAcceptsInput { get; set; } = true;

    /// <inheritdoc />
    public bool HackMenuOverrideConsumed { get; set; }
}
