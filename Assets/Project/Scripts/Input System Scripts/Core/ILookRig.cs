using UnityEngine;

/// <summary>
/// A camera rig that can receive look input and aim state.
/// Implemented by Cinemachine and legacy camera controllers.
/// </summary>
public interface ILookRig
{
    /// <summary>
    /// Apply a look delta (usually from mouse or stick).
    /// </summary>
    void AddLookDelta(Vector2 delta);

    /// <summary>
    /// Set a continuous look rate (usually from joystick).
    /// </summary>
    void SetLookRate(Vector2 rate);

    /// <summary>
    /// Set whether the aim button is held (for zooming/tightening camera).
    /// </summary>
    void SetAimHeld(bool held);
}
