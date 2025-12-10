using UnityEngine;

/// <summary>
/// Interface for systems that need to receive move input (locomotion, combat targeting, etc.)
/// Implemented by KccCharacterController for movement and can be read by other systems.
/// </summary>
public interface IMoveInputReceiver
{
    /// <summary>
    /// Get the current raw move axes (camera-relative, clamped to magnitude 1)
    /// </summary>
    Vector2 MoveAxes { get; }

    /// <summary>
    /// Get the current move input in world space (relative to camera orientation)
    /// </summary>
    Vector3 WorldMoveVector { get; }
}
