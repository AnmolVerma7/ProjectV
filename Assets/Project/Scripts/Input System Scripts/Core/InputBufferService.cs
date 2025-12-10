using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple service to buffer button actions and fire them later when a gate predicate becomes true.
/// </summary>
public sealed class InputBufferService : MonoBehaviour
{
    private struct Item
    {
        public float ExpireAt;
        public Func<bool> Gate;
        public Action Fire;
    }

    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static InputBufferService Instance { get; private set; }

    private readonly List<Item> _items = new List<Item>(16);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        var now = Time.time;
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            var it = _items[i];
            if (now > it.ExpireAt)
            {
                _items.RemoveAt(i);
                continue;
            }
            if (it.Gate())
            {
                _items.RemoveAt(i);
                it.Fire();
            }
        }
    }

    /// <summary>
    /// Buffers an action to be executed when the gate condition becomes true, within a time window.
    /// </summary>
    /// <param name="fire">Action to execute.</param>
    /// <param name="gate">Condition to check every frame.</param>
    /// <param name="windowSeconds">How long to keep trying.</param>
    public void Buffer(Action fire, Func<bool> gate, float windowSeconds)
    {
        if (fire == null || gate == null || windowSeconds <= 0f)
            return;
        _items.Add(
            new Item
            {
                Fire = fire,
                Gate = gate,
                ExpireAt = Time.time + windowSeconds
            }
        );
    }
}
