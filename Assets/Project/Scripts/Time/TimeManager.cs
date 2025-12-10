using UnityEngine;

namespace Antigravity.Time
{
    /// <summary>
    /// Manages the global time state (Rewinding vs Normal).
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        public bool IsRewinding { get; private set; }
        public float MaxRewindSeconds = 5f;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void StartRewind()
        {
            IsRewinding = true;
        }

        public void StopRewind()
        {
            IsRewinding = false;
        }
    }
}
