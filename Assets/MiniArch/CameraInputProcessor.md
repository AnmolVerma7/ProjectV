```csharp
using UnityEngine;

namespace Antigravity.Controllers
{
    /// <summary>
    /// Utility for converting raw input into camera-relative movement vectors.
    /// </summary>
    public static class CameraInputProcessor
    {
        /// <summary>
        /// Converts 2D input (WASD) into a world-space 3D vector relative to the camera's facing direction.
        /// Ensures the vector is flattened on the Y plane (no flying upwards when looking up).
        /// </summary>
        /// <param name="input">The raw X/Y input (usually from InputHandler)</param>
        /// <param name="camera">The camera to be relative to (usually Camera.main)</param>
        /// <returns>World space direction vector</returns>
        public static Vector3 GetCameraRelativeMoveVector(Vector2 input, Camera camera)
        {
            if (camera == null)
            {
                // Fallback if no camera: moves relative to world axes
                return new Vector3(input.x, 0f, input.y);
            }

            // Get camera forward/right directions projected on the ground plane
            Vector3 cameraPlanarDirection = Vector3
                .ProjectOnPlane(camera.transform.forward, Vector3.up)
                .normalized;

            // Edge case: If looking straight up/down, forward projection is zero.
            // Use Up vector as forward in that case.
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3
                    .ProjectOnPlane(camera.transform.up, Vector3.up)
                    .normalized;
            }

            Quaternion cameraPlanarRotation = Quaternion.LookRotation(
                cameraPlanarDirection,
                Vector3.up
            );

            // Rotate input by camera facing
            return cameraPlanarRotation * new Vector3(input.x, 0, input.y);
        }
    }
}
```
