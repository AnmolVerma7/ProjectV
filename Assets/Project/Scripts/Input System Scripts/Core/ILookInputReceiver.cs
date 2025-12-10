using UnityEngine;

/// <summary>
/// Interface for any system that receives raw look input deltas (e.g. mouse movement).
/// </summary>
public interface ILookInputReceiver
{
    /// <summary>
    /// Apply a look delta (usually from mouse or stick).
    /// </summary>
    void AddLookDelta(Vector2 delta);

    /// <summary>
    /// Set a continuous look rate (usually from joystick).
    /// </summary>
    void SetLookRate(Vector2 rate);
}
