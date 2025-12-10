using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [PROJECT SPECIFIC] - EXTENSION
/// This fluent builder wraps the InputRouter for convenience.
/// <para>
/// IT IS COUPLED to the specific ActionMaps defined in InputRouter (e.g. "Player").
/// If your new project has different action maps (e.g. "Vehicle", "Menu"), update the Properties here.
/// </para>
/// </summary>
public class InputBuilder
{
    private readonly InputRouter _router;

    public InputBuilder(InputRouter router)
    {
        _router = router;
    }

    /// <summary>
    /// Access to the raw InputMap actions for strongly-typed binding.
    /// Example: builder.Bind(builder.Actions.Jump)...
    /// </summary>
    public InputMap.PlayerActions Actions => _router.Player;

    /// <summary>
    /// Start binding an action by name (string).
    /// </summary>
    public BindContext Bind(string actionName)
    {
        var action = _router.Input.FindAction(actionName, throwIfNotFound: true);
        return new BindContext(_router, action);
    }

    /// <summary>
    /// Start binding an action by reference (InputAction).
    /// </summary>
    public BindContext Bind(InputAction action)
    {
        return new BindContext(_router, action);
    }

    // --- Fluent Context ---

    public class BindContext
    {
        private readonly InputRouter _router;
        private readonly InputAction _action;

        public BindContext(InputRouter router, InputAction action)
        {
            _router = router;
            _action = action;
        }

        // --- Basic Bindings ---

        /// <summary>
        /// Bind to a simple button press (performed).
        /// </summary>
        public void To(Action onPerformed)
        {
            _router.Bind(_action, new ButtonCommand(onPerformed));
        }

        /// <summary>
        /// Bind with full lifecycle callbacks (Started, Performed, Canceled).
        /// </summary>
        public void To(Action onStarted = null, Action onPerformed = null, Action onCanceled = null)
        {
            _router.Bind(_action, new ButtonCommand(onStarted, onPerformed, onCanceled));
        }

        /// <summary>
        /// Bind to a value (e.g., Vector2 for movement).
        /// </summary>
        public void To<T>(Action<T> onValueChanged)
            where T : struct
        {
            // ValueCommand usually wants specific callbacks, but for simple "just give me the value",
            // we can bind Performed and Started (for initial non-zero) or just Performed.
            // A common pattern for Move is: Started->Set, Performed->Set, Canceled->Zero.

            if (typeof(T) == typeof(Vector2))
            {
                // Specialized handling for Vector2 to ensure zeroing on cancel
                var callback = onValueChanged as Action<Vector2>;
                _router.Bind(
                    _action,
                    new ValueCommand<Vector2>(
                        v => callback(v), // Started
                        v => callback(v), // Performed
                        () => callback(Vector2.zero), // Canceled
                        () => Vector2.zero // Default
                    )
                );
            }
            else if (typeof(T) == typeof(float))
            {
                var callback = onValueChanged as Action<float>;
                _router.Bind(
                    _action,
                    new ValueCommand<float>(
                        v => callback(v),
                        v => callback(v),
                        () => callback(0f),
                        () => 0f
                    )
                );
            }
            else
            {
                // Generic fallback (might not handle zeroing perfectly for all types without more info)
                _router.Bind(
                    _action,
                    new ValueCommand<T>(
                        v => onValueChanged(v),
                        v => onValueChanged(v),
                        () => { },
                        () => default
                    )
                );
            }
        }

        /// <summary>
        /// Bind to the start of the action (Press).
        /// </summary>
        public BindContext Press(Action onPressed)
        {
            _onPress = onPressed;
            return this;
        }

        /// <summary>
        /// Bind to the end of the action (Release).
        /// </summary>
        public BindContext Release(Action onRelease)
        {
            _onRelease = onRelease;
            return this;
        }

        private Action _onPress;
        private Action _onRelease;

        /// <summary>
        /// Finalize the binding for Press/Release chains.
        /// </summary>
        public void Register()
        {
            if (_onPress != null || _onRelease != null)
            {
                // Map Press to Performed (standard for buttons) and Release to Canceled
                _router.Bind(_action, new ButtonCommand(null, _onPress, _onRelease));
            }
        }

        // --- Advanced: Hold / Tap ---

        public HoldTapContext Hold(float duration)
        {
            return new HoldTapContext(_router, _action, duration);
        }

        // --- Advanced: Contextual ---

        public ConditionalContext When(Func<bool> condition)
        {
            return new ConditionalContext(_router, _action, condition);
        }

        // --- Advanced: Buffering ---

        public void Buffer(float bufferDuration, Func<bool> gate, Action onPerformed)
        {
            Action wrappedAction = () =>
            {
                if (gate())
                    onPerformed();
                else
                    InputBufferService.Instance?.Buffer(onPerformed, gate, bufferDuration);
            };

            _router.Bind(_action, new ButtonCommand(wrappedAction));
        }
    }

    public class HoldTapContext
    {
        private readonly InputRouter _router;
        private readonly InputAction _action;
        private readonly float _holdDuration;

        private Action _onTap;
        private Action _onHold;
        private Action _onPress;
        private Action _onRelease;

        public HoldTapContext(InputRouter router, InputAction action, float holdDuration)
        {
            _router = router;
            _action = action;
            _holdDuration = holdDuration;
        }

        public HoldTapContext Tap(Action onTap)
        {
            _onTap = onTap;
            return this;
        }

        public HoldTapContext To(Action onHold)
        {
            _onHold = onHold;
            return this;
        } // "Hold().To()" reads well

        public HoldTapContext Press(Action onPress)
        {
            _onPress = onPress;
            return this;
        }

        public HoldTapContext Release(Action onRelease)
        {
            _onRelease = onRelease;
            return this;
        }

        public void Build()
        {
            _router.Bind(
                _action,
                new HoldTapReleaseCommand(
                    holdThresholdSeconds: _holdDuration,
                    onTap: _onTap,
                    onHold: _onHold,
                    onPressed: _onPress,
                    onReleased: _onRelease
                )
            );
        }

        public void Register() => Build();
    }

    public class ConditionalContext
    {
        private readonly InputRouter _router;
        private readonly InputAction _action;
        private readonly Func<bool> _condition;
        private IInputCommand _whenTrue;
        private IInputCommand _whenFalse;

        public ConditionalContext(InputRouter router, InputAction action, Func<bool> condition)
        {
            _router = router;
            _action = action;
            _condition = condition;
        }

        public ConditionalContext To(Action onTrue)
        {
            _whenTrue = new ButtonCommand(onTrue);
            return this;
        }

        public ConditionalContext Otherwise(Action onFalse)
        {
            _whenFalse = new ButtonCommand(onFalse);
            return this;
        }

        public void Register()
        {
            _router.Bind(_action, new ConditionalCommand(_condition, _whenTrue, _whenFalse));
        }
    }
}
