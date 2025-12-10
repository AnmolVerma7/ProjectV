using System.Collections.Generic;
using UnityEngine;

namespace Antigravity.Time
{
    [System.Serializable]
    public struct PointInTime
    {
        public Vector3 position;
        public Quaternion rotation;
        public float time;

        public PointInTime(Vector3 pos, Quaternion rot, float t)
        {
            position = pos;
            rotation = rot;
            time = t;
        }
    }

    /// <summary>
    /// Attach this to any object (Player, Box, Enemy) to make it rewindable.
    /// </summary>
    public class RewindableObject : MonoBehaviour
    {
        private List<PointInTime> _pointsInTime;
        private Rigidbody _rb;

        private void Awake()
        {
            _pointsInTime = new List<PointInTime>();
            _rb = GetComponent<Rigidbody>();
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
            // Record current state
            // We record in FixedUpdate for physics consistency
            if (
                _pointsInTime.Count
                > Mathf.Round(
                    TimeManager.Instance.MaxRewindSeconds / UnityEngine.Time.fixedDeltaTime
                )
            )
            {
                _pointsInTime.RemoveAt(_pointsInTime.Count - 1); // Remove oldest
            }

            _pointsInTime.Insert(
                0,
                new PointInTime(transform.position, transform.rotation, UnityEngine.Time.time)
            );
        }

        private void Rewind()
        {
            if (_pointsInTime.Count > 0)
            {
                PointInTime point = _pointsInTime[0];
                transform.position = point.position;
                transform.rotation = point.rotation;
                _pointsInTime.RemoveAt(0);
            }
            else
            {
                // No more history, stop rewinding?
                // Or just stay frozen.
                TimeManager.Instance.StopRewind();
            }
        }
    }
}
