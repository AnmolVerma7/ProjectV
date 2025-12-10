/// <summary>
/// [PROJECT SPECIFIC]
/// Defines the abstract "Vocabulary" of combo buttons for this specific game.
/// You might change "One/Two" to "Light/Heavy" or "Punch/Kick" depending on your game.
/// Mapped via the router: 1 = AttackOrInteract, 2 = TeleportAttack.
/// </summary>
public enum ComboButton
{
    /// <summary>Primary attack button.</summary>
    One = 1,

    /// <summary>Secondary/Teleport attack button.</summary>
    Two = 2
}
