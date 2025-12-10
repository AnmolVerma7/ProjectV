# 🎮 Player Controller Guide

Welcome to the **Antigravity Player Controller**! This system has been refactored (Dec 2025) to be modular, learnable, and robust.

## 🏗️ Architecture Overview

We follow the **Separation of Concerns** principle. The "Player" is not one script, but a team of three:

1.  **🧠 Input Handling (`PlayerInputHandler.cs`)**
    - **Role**: The "Ears". Listens to Unity's Input System.
    - **Key Feature**: Separates **Mouse Delta** (Pixels) from **Gamepad Rate** (Analog Stick) for consistent camera speed.
    - **Data**:
      - `MoveInput` (Vector2)
      - `LookDelta` (Vector2) - _Use for Mouse_
      - `LookRate` (Vector2) - _Use for Gamepad_
      - `JumpDown`, `JumpHeld`, `CrouchHeld`, etc.
    - **Execution Order**: set to `-100` to run _before_ the controller.
2.  **🧬 The DNA (`PlayerMovementConfig`)**
    - **Role**: Stores tuning values (`Speed`, `Gravity`, `JumpForce`).
    - **Type**: `ScriptableObject` (Asset file).
    - **Location**: `Assets/Project/Scripts/Controllers/PlayerMovementConfig.cs`
3.  **💪 The Muscles (`PlayerController`)**
    - **Role**: Physics & Movement Logic.
    - **Input**: Reads from **Brain** + **DNA**.
    - **Location**: `Assets/Project/Scripts/Controllers/PlayerController.cs`

---

## 🚀 Features & Controls

### 1. Movement

- **WASD / Left Stick**: Move.
- **Space / South Button**: Jump.
  - _Supports Double Jump & Wall Jump (Configurable)._
- **C / East Button**: Crouch / Slide.

### 2. Time Rewind ⏳

- **Hold Left Click / Right Trigger**: Rewind Time.
- _Logic_: The `PlayerInputHandler` detects the hold and tells `TimeManager` to start rewinding. The Controller pauses its physics simulation while rewinding.

### 3. Developer Cheats 🛠️

- **N Key**: Toggle **Noclip Mode**.
  - Fly through walls.
  - Gravity disabled.
  - _Note_: This input is handled directly in `PlayerInputHandler` to bypass the complex Input System for simple debugging.

---

## 🛠️ How to Configure

### Creating a New Movement Preset

1.  Right-Click in the Project Window.
2.  Select **Antigravity** -> **Player Movement Config**.
3.  Name it (e.g., `FastScoutConfig`).
4.  Tweak the values in the Inspector.

### Assigning to Player

1.  Select your **Player** GameObject.
2.  Find the **Player Controller** component.
3.  Drag your new Config asset into the **Config** slot.

---

## 📝 Change Log

### [2025-12-04] Refactoring & Cleanup

- **Extracted Input**: Moved all `InputBuilder` code out of Controller into `PlayerInputHandler`.
- **Extracted Config**: Moved all `public float` settings into `PlayerMovementConfig` ScriptableObject.
- **Fixed Noclip**: Restored Noclip functionality after refactor.
- **Cleaned Up**: Removed unused legacy code and comments.

## 🎓 Learning Moment: Script Execution Order

We encountered a bug where inputs weren't being detected. This was a **Race Condition**.

### The Problem

1.  Unity runs `Update()` on all scripts, but the **order is random** by default.
2.  **Scenario**:
    - `PlayerController` runs first. Checks input. Input is `false`.
    - `InputHandler` runs second. Detects key press. Sets input to `true`.
    - `InputHandler` runs `LateUpdate`. Resets input to `false`.
    - **Result**: The Controller missed the input entirely!

### The Fix

We added `[DefaultExecutionOrder(-100)]` to `PlayerInputHandler.cs`.

- This forces Unity to run `InputHandler` **before** everything else.
- **New Flow**: InputHandler (True) -> PlayerController (Reads True) -> LateUpdate (Reset).

_Always ensure your Data Providers run before your Data Consumers!_

---

_This document tracks the evolution of the Player Controller. Update it when adding new abilities!_
