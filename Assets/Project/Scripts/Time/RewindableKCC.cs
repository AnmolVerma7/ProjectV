using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

namespace Antigravity.Time
{
    /// <summary>
    /// A specialized version of RewindableObject for KinematicCharacterController.
    /// Saves the full motor state (Position, Rotation, Velocity, Grounding) for glitch-free rewinding.
    /// </summary>
    [RequireComponent(typeof(KinematicCharacterMotor))]
    public class RewindableKCC : MonoBehaviour
    {
        private KinematicCharacterMotor _motor;
        private List<KinematicCharacterMotorState> _history;

        private void Awake()
        {
            _motor = GetComponent<KinematicCharacterMotor>();
            _history = new List<KinematicCharacterMotorState>();
        }

        private void FixedUpdate()
        {
            if (TimeManager.Instance == null)
                return;

            if (TimeManager.Instance.IsRewinding)
            {
                Rewind();
            }
            else
            {
                Record();
            }
        }

        private void Record()
        {
            if (
                _history.Count
                > Mathf.Round(
                    TimeManager.Instance.MaxRewindSeconds / UnityEngine.Time.fixedDeltaTime
                )
            )
            {
                _history.RemoveAt(_history.Count - 1);
            }

            // KCC provides a built-in method to get the full state
            _history.Insert(0, _motor.GetState());
        }

        private void Rewind()
        {
            if (_history.Count > 0)
            {
                KinematicCharacterMotorState state = _history[0];

                // Apply the state to the motor (bypassing interpolation for instant snap)
                _motor.ApplyState(state, bypassInterpolation: true);

                _history.RemoveAt(0);
            }
            else
            {
                TimeManager.Instance.StopRewind();
            }
        }
    }
}
