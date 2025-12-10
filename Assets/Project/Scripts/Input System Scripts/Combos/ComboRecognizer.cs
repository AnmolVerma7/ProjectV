using System;
using UnityEngine;

/// <summary>
/// Recognizes simple button sequences with timing rules and integrates early input buffering
/// using InputBufferService. Designed for small combo sets (like 1,1,2,2).
/// </summary>
public sealed class ComboRecognizer
{
    /// <summary>
    /// Defines a single combo sequence.
    /// </summary>
    [Serializable]
    public sealed class Combo
    {
        /// <summary>Unique name for the combo.</summary>
        public string Name;

        /// <summary>Sequence of buttons required.</summary>
        public ComboButton[] Steps;

        /// <summary>Minimum delay between steps (to prevent accidental double-presses).</summary>
        public float MinStepDelay = 0.05f;

        /// <summary>Maximum delay between steps (reset if exceeded).</summary>
        public float MaxStepDelay = 0.6f;

        /// <summary>How long to buffer an early press if it comes before minStepDelay.</summary>
        public float EarlyBufferWindow = 0.2f;

        /// <summary>Callback when sequence completes.</summary>
        public Action OnTriggered;

        // Runtime state
        [NonSerialized]
        public int Index; // Next expected step index

        [NonSerialized]
        public float LastTime; // Time of the last accepted step
    }

    private readonly Combo[] _combos;

    public ComboRecognizer(params Combo[] combos)
    {
        _combos = combos ?? Array.Empty<Combo>();
        var now = Time.time;
        foreach (var c in _combos)
        {
            c.Index = 0;
            c.LastTime = now;
        }
    }

    /// <summary>
    /// Register an input press. Call this from action Performed handlers.
    /// </summary>
    public void Register(ComboButton button)
    {
        var now = Time.time;
        for (int i = 0; i < _combos.Length; i++)
        {
            var c = _combos[i];
            if (c.Steps == null || c.Steps.Length == 0)
                continue;

            // If too slow since last accepted step, reset progress.
            if (c.Index > 0 && now - c.LastTime > c.MaxStepDelay)
            {
                c.Index = 0;
            }

            if (c.Index == 0)
            {
                // Start sequence if first step matches
                if (button == c.Steps[0])
                {
                    c.Index = 1;
                    c.LastTime = now;
                }
                // else: ignore for this combo
                continue;
            }

            // We are mid-sequence: enforce min delay and max delay
            var dt = now - c.LastTime;
            if (dt < c.MinStepDelay)
            {
                // Too fast: buffer this press until the min delay passes.
                var nextAllowed = c.LastTime + c.MinStepDelay;
                InputBufferService.Instance?.Buffer(
                    fire: () => Register(button),
                    gate: () => Time.time >= nextAllowed,
                    windowSeconds: c.EarlyBufferWindow
                );
                continue;
            }

            // At/after min delay
            if (button == c.Steps[c.Index])
            {
                c.Index++;
                c.LastTime = now;
                if (c.Index >= c.Steps.Length)
                {
                    // Trigger combo and reset
                    c.OnTriggered?.Invoke();
                    c.Index = 0;
                    c.LastTime = now;
                }
            }
            else
            {
                // Mismatch: attempt to treat this press as a potential new start
                c.Index = 0;
                if (button == c.Steps[0])
                {
                    c.Index = 1;
                    c.LastTime = now;
                }
            }
        }
    }
}
