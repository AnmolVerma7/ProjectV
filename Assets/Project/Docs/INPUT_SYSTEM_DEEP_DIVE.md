# 🧠 Input System: Deep Dive

> **Purpose**: This document explains every script in the `Input System Scripts` folder. Use this to understand the architecture, extend functionality, or debug complex issues.

---

## 📂 Core (`/Core`)

The nervous system of the input architecture.

### `InputRouter.cs`

- **Role**: The Central Hub.
- **What it does**:
  - Owns the `InputMap` (Unity's generated C# class).
  - Manages the lifecycle (Enable/Disable) of input actions.
  - Dispatches events to registered `IInputCommand`s.
- **Why keep it**: It decouples your code from Unity's specific implementation. You bind to _this_, not Unity directly.

### `InputBufferService.cs`

- **Role**: The Memory.
- **What it does**: Stores an input action (like Jump) if it was pressed "too early" (e.g., while still in the air). When the condition becomes valid (landing), it automatically fires the action.
- **Why keep it**: Essential for "Fluid" combat and platforming. Without it, controls feel unresponsive.

### `IInputCommand.cs`

- **Role**: The Contract.
- **What it does**: Defines the single method `Execute(CallbackContext ctx)` that all commands must implement.

### `ILookRig.cs` / `ILookInputReceiver.cs` / `IMoveInputReceiver.cs`

- **Role**: The Standards (Interfaces).
- **What it does**: Defines _how_ a camera or character should receive input.
  - `AddLookDelta(Vector2)`: For Mouse (accumulate movement).
  - `SetLookRate(Vector2)`: For Gamepad (continuous speed).
- **Why keep it**: Even if unused now, these are critical for future "Vehicle" or "Turret" systems where you want to swap control schemes without rewriting input logic.

---

## 📂 Commands (`/Commands`)

The "Verbs" of the system. These translate raw Unity events into game logic.

### `ButtonCommand.cs`

- **Role**: Simple Press.
- **Usage**: `builder.Bind(...).To(Jump)`
- **Logic**: Fires callbacks for Started (Down), Performed (Held/Up), and Canceled (Up).

### `ValueCommand.cs`

- **Role**: Continuous Data.
- **Usage**: `builder.Bind(...).To<Vector2>(Move)`
- **Logic**: Passes raw values (float, Vector2) to your code.

### `LookInputCommand.cs` 🌟

- **Role**: Smart Camera Input.
- **Logic**: Automatically detects the device:
  - **Mouse**: Calls `AddDelta` (Frame-based).
  - **Gamepad**: Calls `SetRate` (Time-based).
- **Why it's cool**: Solves the classic "Why does my joystick snap the camera?" bug.

### `HoldTapReleaseCommand.cs`

- **Role**: Timing Logic.
- **Logic**: Distinguishes between a quick tap and a long hold.
- **Usage**: Tap to Interact, Hold to Open Menu.

### `ConditionalCommand.cs`

- **Role**: The Gatekeeper.
- **Logic**: Checks a `Func<bool>` before executing.
- **Usage**: "Only Jump if `!IsStunned`".

### `CompositeCommand.cs`

- **Role**: The Grouper.
- **Logic**: Executes multiple commands from a single input.

---

## 📂 Combos (`/Combos`)

The "Fighting Game" logic.

### `ComboRecognizer.cs`

- **Role**: The Pattern Matcher.
- **Logic**: Listens to a stream of inputs (A, A, B) and checks them against a list of defined moves.
- **Features**: Supports "Early Buffer" (inputting the next move before the current one finishes).

---

## 📂 Root Scripts

### `InputBuilder.cs`

- **Role**: The Fluent API.
- **What it does**: Wraps the complex `InputRouter` and `Command` instantiation into readable, English-like sentences.
- **Example**: `.Bind(Jump).Buffer(0.2f, IsGrounded, DoJump)`

### `GameInputContext.cs`

- **Role**: The State Container.
- **What it does**: A simple class to hold flags like `IsMenuOpen` or `HasTarget`.
- **Why keep it**: Allows input commands to check game state without referencing the Player Controller directly.

### `IInputUser.cs`

- **Role**: The Client.
- **What it does**: Any script that wants input (Player, Vehicle, Menu) implements this to get a reference to the `InputBuilder`.
