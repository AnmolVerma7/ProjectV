# Movement System Architecture Documentation

**Project:** AntigravityUnityGame  
**Pattern:** Handler-Based Composition (Priority-Free)  
**Framework:** Kinematic Character Controller (KCC)

---

## Table of Contents

1. [Core Architecture](#core-architecture)
2. [System Components](#system-components)
3. [Handler Implementations](#handler-implementations)
4. [Input System](#input-system)
5. [Configuration System](#configuration-system)
6. [Animation System](#animation-system)
7. [File Structure](#file-structure)
8. [Design Principles](#design-principles)

---

## Core Architecture

### The Pattern: Handler-Based Composition

**Philosophy:** One coordinator (DefaultMovement) delegates to specialized handlers. No priority checks, no modules fighting for control.

```
PlayerController (Entry Point)
    ↓
DefaultMovement (Coordinator/Brain)
    ├── JumpHandler (All jump logic)
    ├── SlideHandler (All slide logic)
    ├── DashHandler (All dash logic)
    ├── MantleHandler (All mantle/shimmy logic)
    └── PlayerAnimator (Animation bridge)
```

### Key Characteristics

1. **Single Coordinator:** DefaultMovement owns the physics update loop
2. **Pure Handlers:** Handlers answer questions and modify state, don't control flow
3. **Clear Data Flow:** Input → Controller → DefaultMovement → Handlers → Physics
4. **No Priorities:** Execution order is explicit, not priority-based

---

## System Components

### 1. DefaultMovement (The Brain)

**File:** `DefaultMovement.cs`  
**Role:** Coordinates all movement logic in a single UpdateVelocity loop

**Core Flow:**

```csharp
public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
{
    // 1. Special state override (mantle has full control)
    if (_mantleHandler.IsActive) {
        _mantleHandler.UpdateMantle(ref currentVelocity, deltaTime);
        return; // Early exit
    }

    // 2. Ground vs Air movement
    if (Motor.GroundingStatus.IsStableOnGround)
        ApplyGroundMovement(ref currentVelocity, deltaTime);
    else
        ApplyAirMovement(ref currentVelocity, deltaTime);

    // 3. Jump processing (works in both states)
    _jumpHandler.ProcessJump(ref currentVelocity, deltaTime, _moveInputVector);

    // 4. Mantle grab attempts
    if (shouldTryGrab)
        _mantleHandler.TryGrab();

    // 5. Apply accumulated impulses (dash, etc.)
    ApplyInternalVelocity(ref currentVelocity);
}
```

**Magic Numbers Section:**

```csharp
// All tunable values extracted to constants at top of file
private const float DASH_VELOCITY_THRESHOLD = 0.1f;
private const float AIR_SPEED_DECAY_SHARPNESS = 10f;
// etc.
```

---

## Handler Implementations

### JumpHandler

**File:** `JumpHandler.cs`  
**Responsibility:** All jump types (ground, coyote, air/double, wall)

**Features:**

- Multi-jump counter system (scalable to triple/quad jumps)
- Coyote time (JumpPostGroundingGraceTime)
- Jump buffering (JumpPreGroundingGraceTime)
- Wall jump support
- **Scalable jump velocity** (forward momentum on jump)

**Key API:**

```csharp
void RequestJump()                                    // Buffer jump intent
void ProcessJump(ref Vector3 vel, float dt, Vector3 input) // Main loop
void OnWallHit(Vector3 wallNormal)                   // Enable wall jump
bool JumpConsumedThisUpdate { get; }                 // Query state
```

**Physics:**

```csharp
// Vertical impulse
currentVelocity += (jumpDirection * upSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);

// Forward impulse (Titanfall-style momentum)
if (moveInputVector.sqrMagnitude > 0f) {
    currentVelocity += moveInputVector * forwardSpeed;
}
```

**Config Values:**

- `JumpSpeed` - Vertical speed for ground/coyote jumps
- `JumpScalableForwardSpeed` - Forward kick on ground jump
- `DoubleJumpSpeed` - Independent vertical speed for air jumps
- `DoubleJumpScalableForwardSpeed` - Independent forward kick

---

### SlideHandler

**File:** `SlideHandler.cs`  
**Responsibility:** Momentum-based sliding with physics decay

**Features:**

- Entry speed boost
- Friction curve decay over time
- Slope influence (faster downhill, slower uphill)
- Player steering while sliding
- Auto-exit on min speed or max duration

**Key API:**

```csharp
void RequestSlide()                              // Intent to slide
void HandleSlide()                               // Check entry/exit conditions
void ApplySlidePhysics(ref Vector3 vel, float dt) // Physics calculation
bool IsSliding { get; }                          // State query
```

**Physics Model:**

```csharp
// 1. Capture initial speed with boost
_initialSlideSpeed = currentSpeed * _config.SlideSpeedBoost;

// 2. Apply friction curve
float slideProgress = _slideTimer / _config.MaxSlideDuration;
float frictionFactor = _config.SlideFrictionCurve.Evaluate(slideProgress);
float currentSlideSpeed = _initialSlideSpeed * frictionFactor;

// 3. Apply slope influence
float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
float slopeFactor = (slopeAngle - 90f) / 90f; // -1 to +1
currentSlideSpeed += slopeFactor * _config.SlopeInfluence;

// 4. Add player steering
Vector3 steerVelocity = steerInput * _config.SlideSteerStrength;
```

**Config Values:**

- `SlideSpeedBoost` - Initial speed multiplier (1.2 = 20% boost)
- `MaxSlideDuration` - Max time before auto-exit
- `SlideFrictionCurve` - AnimationCurve for decay (0=start, 1=end)
- `SlopeInfluence` - How strongly slope affects speed
- `SlideSteerStrength` - How much player can turn
- `MinSlideExitSpeed` - Speed threshold to auto-exit

---

### DashHandler

**File:** `DashHandler.cs`  
**Responsibility:** Charge-based dash with cooldown

**Features:**

- Multi-charge system (configurable max charges)
- Auto-reload timer per charge
- Intermission cooldown (spam prevention)
- **Universal dash distance** (consistent regardless of velocity)

**Key API:**

```csharp
void RequestDash()                                    // Intent to dash
bool TryApplyDash(ref Vector3 velocityAdd, Vector3 direction) // Apply if valid
void UpdateCharges(float deltaTime)                  // Reload logic
int CurrentDashCharges { get; }                       // State query
```

**Physics (Critical Detail):**

```csharp
// Original naive approach (inconsistent distance):
velocityAdd += direction * _config.DashForce;

// Current approach (consistent distance):
// 1. Cancel existing velocity in dash direction
Vector3 dashVelocityToCancel = Vector3.Project(velocityAdd, direction);
velocityAdd -= dashVelocityToCancel;

// 2. Apply full dash force
velocityAdd += direction * _config.DashForce;
```

**Ground vs Air Dash Direction:**

- **Ground:** Uses current velocity direction (or facing if stationary)
- **Air:** Uses raw input direction (for aerial redirection)

**Config Values:**

- `DashForce` - Absolute dash velocity (not additive)
- `MaxDashCharges` - Number of charges available
- `DashReloadTime` - Seconds to reload one charge
- `DashIntermissionTime` - Cooldown between dashes

---

### MantleHandler

**File:** `MantleHandler.cs`  
**Responsibility:** Ledge detection, hanging, shimmying, mantling

**Features:**

- Raycast-based ledge detection (no trigger colliders)
- Grab → Hang → Mantle (or drop)
- Shimmy left/right along ledges
- Drop with cooldown to prevent immediate re-grab
- Arc-based mantle motion (natural "pull up" feel)

**State Machine:**

```
None → Grabbing → Hanging → Mantling → None
         ↑           ↓
         └─────── Drop
```

**Key API:**

```csharp
bool CanGrab()                                    // Ledge detection check
void RequestMantle()                              // Confirm mantle from hang
void RequestDrop()                                // Drop from ledge
void UpdateMantle(ref Vector3 vel, float dt)      // Full velocity override
MantleState CurrentState { get; }                 // State query
```

**Detection Logic:**

```csharp
// 1. Forward raycast for wall
Physics.Raycast(position, forward, out wallHit, MaxGrabDistance);

// 2. Down raycast from above wall for ledge top
Vector3 aboveWall = wallHit.point + (up * MaxLedgeHeight);
Physics.Raycast(aboveWall, -up, out ledgeHit, MaxLedgeHeight);

// 3. Validate walkability
float slopeAngle = Vector3.Angle(Vector3.up, ledgeHit.normal);
if (slopeAngle > 45f) return false; // Too steep
```

**Shimmy Detection:**

```csharp
// OverlapSphere at edge to check for continuation
Vector3 checkPos = grabPos + (ledgeRight * ShimmyCheckDistance);
if (Physics.OverlapSphere(checkPos, radius).Length == 0)
    return false; // Edge/gap detected
```

**Magic Numbers (TODO: Move to config):**

```csharp
private const float SHIMMY_CHECK_VERTICAL = 0.3f;
private const float SHIMMY_CHECK_FORWARD = 0.5f;
private const float SHIMMY_SPHERE_RADIUS = 0.3f;
private const float GRAB_PULLBACK = 0.05f;
private const float MANTLE_FORWARD_OFFSET = 0.15f;
private const float DROP_COOLDOWN = 0.5f;
```

**Config Values:**

- `MantleLayers` - LayerMask for detection optimization
- `MaxGrabDistance` - Horizontal detection range
- `MinLedgeHeight` / `MaxLedgeHeight` - Valid height range
- `MantleDuration` - Mantle animation length
- `MantleCurve` - AnimationCurve for motion smoothness
- `ShimmySpeed` - Movement speed along ledge
- `ShimmyCheckDistance` - Edge detection range

---

## Input System

### PlayerInputHandler

**File:** `PlayerInputHandler.cs`  
**Pattern:** Store intent, don't trigger actions

**Philosophy:**

```
❌ BAD:  Input → Immediate Action (couples input to logic)
✅ GOOD: Input → Store Flags → Physics reads flags (decoupled)
```

**Key Properties:**

```csharp
// Movement Input
Vector3 MoveInput { get; }           // Analog stick/WASD (world-space)
Vector3 CameraRelativeMoveInput { get; } // Relative to camera

// Action Flags (consumed by movement system)
bool JumpDown { get; }               // Jump button pressed this frame
bool DashDown { get; }               // Dash button pressed
bool IsSprinting { get; }            // Sprint toggle/hold state
bool ShouldBeCrouching { get; }      // Crouch toggle/hold state

// Special
bool CrouchJustActivated { get; }    // For slide entry detection
```

**Toggle vs Hold Support:**

```csharp
// Sprint example
if (_config.ToggleSprint) {
    if (sprintInput) _sprintToggle = !_sprintToggle;
} else {
    _sprintToggle = sprintInput; // Hold mode
}
```

**Integration:**

```csharp
// PlayerController.Update() reads input
InputHandler.UpdateInput(); // Store current frame's input

// DefaultMovement.UpdateVelocity() uses it
SetMoveInput(InputHandler.MoveInput);
if (InputHandler.JumpDown) RequestJump();
if (InputHandler.DashDown) _dashHandler.RequestDash();
```

---

## Configuration System

### PlayerMovementConfig

**File:** `PlayerMovementConfig.cs`  
**Type:** ScriptableObject (inspector-editable asset)

**Benefits:**

1. All magic numbers in one place
2. Tweak without recompiling
3. Create presets (Fast/Slow/Bouncy variants)
4. Easy A/B testing

**Sections:**

```csharp
[Header("Ground Movement")]
public float MaxStableMoveSpeed = 8f;
public float MaxSprintMoveSpeed = 15f;
public float StableMovementSharpness = 15f;

[Header("Air Movement")]
public float MaxAirMoveSpeed = 20f;
public float AirAccelerationSpeed = 20f;
public float Drag = 0.1f;

[Header("Jumping")]
public float JumpSpeed = 10f;
public float JumpScalableForwardSpeed = 10f;
public float DoubleJumpSpeed = 10f;
public float DoubleJumpScalableForwardSpeed = 10f;
public float JumpPreGroundingGraceTime = 0.15f;
public float JumpPostGroundingGraceTime = 0.1f;

[Header("Slide - Momentum Physics")]
public float SlideSpeedBoost = 1.2f;
public AnimationCurve SlideFrictionCurve;
// etc...

[Header("Dash")]
public float DashForce = 15f;
public int MaxDashCharges = 3;
public float DashReloadTime = 2.0f;

[Header("Mantle")]
public LayerMask MantleLayers = -1;
public float MaxGrabDistance = 0.3f;
public AnimationCurve MantleCurve;
// etc...
```

**Usage:**

```csharp
// In handlers
float jumpForce = _config.JumpSpeed;
```

---

## Animation System

### PlayerAnimator

**File:** `PlayerAnimator.cs`  
**Role:** Bridge between physics and Unity Animator

**Philosophy:** Read physics state → Set animator parameters → Animator drives visuals

**Parameters (Standardized):**

```csharp
// Floats
Speed              // Horizontal velocity magnitude
VerticalSpeed      // Y-axis velocity
InputMagnitude     // Analog stick tilt amount (0-1)
AnimSpeed          // Playback multiplier for foot sync

// Bools
IsGrounded
IsCrouching
IsSliding
IsHanging
IsMantling

// Triggers
Jump
Land
```

**Update Flow:**

```csharp
void Update() {
    // Read from Motor
    float horizontalSpeed = Vector3.ProjectOnPlane(Motor.Velocity, Motor.CharacterUp).magnitude;
    float verticalSpeed = Vector3.Dot(Motor.Velocity, Motor.CharacterUp);

    // Set parameters
    Animator.SetFloat(PARAM_SPEED, horizontalSpeed);
    Animator.SetFloat(PARAM_VERTICAL_SPEED, verticalSpeed);
    Animator.SetFloat(PARAM_INPUT_MAGNITUDE, inputMagnitude);

    // Calculate playback speed (matches footsteps to actual movement)
    float animSpeed = CalculateAnimSpeed(horizontalSpeed);
    Animator.SetFloat(PARAM_ANIM_SPEED, animSpeed);
}
```

**Speed Matching Algorithm:**

```csharp
float CalculateAnimSpeed(float currentSpeed) {
    if (currentSpeed < _idleThreshold) return 1f;

    // Determine which animation is playing
    float clipSpeed = currentSpeed < _sprintThreshold ? _jogClipSpeed : _sprintClipSpeed;

    // Scale playback to match actual speed
    return Mathf.Clamp(currentSpeed / clipSpeed, 0.5f, 2f);
}
```

**Animator Controller Setup (Manual):**

```
Base Layer
├─ Locomotion (Blend Tree - 1D, Speed parameter)
│   ├─ Idle (threshold: 0)
│   ├─ Walk (threshold: 2)
│   ├─ Jog (threshold: 5)
│   └─ Sprint (threshold: 12)
├─ Jump State
├─ Fall State
└─ Crouch/Slide States

Transitions:
- AnyState → Jump (Condition: Jump trigger)
- Jump → Fall (Condition: VerticalSpeed < -0.5)
- Fall → Locomotion (Condition: IsGrounded = true)
```

**Integration:**

```csharp
// PlayerController wires up calls
PlayerAnimator.SetInputMagnitude(InputHandler.MoveInput.magnitude);
PlayerAnimator.TriggerJump();            // On jump
PlayerAnimator.SetSliding(_defaultMovement.IsSliding);
```

---

## File Structure

```
Assets/Project/Scripts/
├── Controllers/
│   ├── PlayerController.cs           (Entry point, Update loop)
│   ├── PlayerMovementConfig.cs       (ScriptableObject config)
│   └── PlayerInputHandler.cs         (Input storage)
│
├── Movement/
│   ├── MovementModuleBase.cs         (Abstract base for modules)
│   ├── DefaultMovement.cs            (Main coordinator/brain)
│   ├── JumpHandler.cs                (All jump logic)
│   ├── SlideHandler.cs               (Slide physics)
│   ├── DashHandler.cs                (Dash system)
│   └── MantleHandler.cs              (Mantle/shimmy/drop)
│
└── Character/
    ├── PlayerAnimator.cs             (Animation bridge)
    └── States/
        ├── PlayerStateMachine.cs     (High-level state tracking)
        ├── PlayerBaseState.cs        (State pattern base)
        └── [Various state files]    (Idle, Move, Jump, etc.)
```

**Note:** The States/ folder currently serves as **observation only** - it tracks state but doesn't control physics. Physics control is in DefaultMovement/Handlers.

---

## Design Principles

### 1. Composition Over Inheritance

**Why:**

- Easier to reason about
- Handlers are reusable
- No deep class hierarchies

**Example:**

```csharp
// ❌ BAD: Module inheritance
class SlideModule : MovementModuleBase { }
class WallRunModule : MovementModuleBase { }

// ✅ GOOD: Handler composition
class DefaultMovement {
    private SlideHandler _slideHandler;
    private WallRunHandler _wallRunHandler;
}
```

### 2. Single Responsibility

Each handler does **one thing only:**

- JumpHandler = Jumping
- SlideHandler = Sliding
- DashHandler = Dashing

### 3. Ask, Don't Tell

Handlers answer questions, don't control:

```csharp
// ❌ BAD: Handler controls flow
handler.Activate(); // What does this do?

// ✅ GOOD: Coordinator asks handler
if (handler.CanSlide()) {
    handler.ApplySlidePhysics(ref vel, dt);
}
```

### 4. Explicit Over Implicit

No hidden priority checks:

```csharp
// ❌ BAD: Priority system (implicit order)
foreach (var module in sorted_by_priority) {
    if (module.CanTakeControl()) break;
}

// ✅ GOOD: Explicit flow (readable order)
if (mantling) HandleMantle();
else if (grounded) HandleGround();
else HandleAir();
```

### 5. Configuration as Data

All tunable values in ScriptableObject:

- No hardcoded numbers in logic
- Easy to tweak/test
- Version controllable presets

---

## Air Control Implementation

### Source Engine Style Air Strafing

**Config Values:**

- `MaxAirMoveSpeed` = 20f (cap during normal air movement)
- `AirAccelerationSpeed` = 20f (how fast you can turn)

**Physics:**

```csharp
if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed) {
    // Below cap: add velocity up to cap
    Vector3 newTotal = Vector3.ClampMagnitude(currentVelocity + addedVelocity, MaxAirMoveSpeed);
} else {
    // Over cap (from dash/jump): allow perpendicular steering only
    if (Vector3.Dot(currentVelocity, addedVelocity) > 0f) {
        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocity.normalized);
    }
}
```

**Decay Over Max Speed:**

```csharp
// When over MaxAirMoveSpeed (e.g., from dash), decay slowly back to cap
if (currentPlanarSpeed > MaxAirMoveSpeed) {
    currentVelocity = Vector3.Lerp(
        currentVelocity,
        targetVelocity,
        1 - Mathf.Exp(-AIR_SPEED_DECAY_SHARPNESS * deltaTime)
    );
}
```

---

## Integration Points

### PlayerController → DefaultMovement

```csharp
// PlayerController.Update()
InputHandler.UpdateInput();

// Pass to DefaultMovement
_defaultMovement.SetMoveInput(InputHandler.MoveInput);

// Process actions
if (InputHandler.JumpDown) {
    if (_defaultMovement.IsMantling)
        _defaultMovement.RequestMantleConfirm();
    else
        _defaultMovement.RequestJump();
}

if (InputHandler.DashDown)
    _dashHandler.RequestDash();

if (InputHandler.CrouchJustActivated)
    _defaultMovement.RequestSlide();
```

### DefaultMovement → KCC

```csharp
// Motor calls these during physics update
public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
public override void AfterCharacterUpdate(float deltaTime)
```

### DefaultMovement → Animator

```csharp
// PlayerController.Update() bridges state to animator
PlayerAnimator.SetInputMagnitude(InputHandler.MoveInput.magnitude);
PlayerAnimator.SetSliding(_defaultMovement.IsSliding);
PlayerAnimator.SetGrounded(Motor.GroundingStatus.IsStableOnGround);

// Handlers trigger one-shots
if (jumpExecuted) PlayerAnimator.TriggerJump();
```

---

## Common Patterns

### Handler Request Pattern

```csharp
// 1. Player presses button
InputHandler.JumpDown = true;

// 2. Controller reads input
if (InputHandler.JumpDown) _defaultMovement.RequestJump();

// 3. DefaultMovement forwards to handler
public void RequestJump() => _jumpHandler.RequestJump();

// 4. Handler stores intent (doesn't execute yet)
public void RequestJump() => _jumpRequested = true;

// 5. Physics update processes it
_jumpHandler.ProcessJump(ref velocity, deltaTime, moveInput);
```

### State Query Pattern

```csharp
// Handlers expose read-only state
public bool IsSliding { get; }
public bool IsMantling { get; }
public int CurrentDashCharges { get; }

// Coordinator uses queries to make decisions
if (_slideHandler.IsSliding) {
    // Sliding has priority over normal ground movement
    _slideHandler.ApplySlidePhysics(ref velocity, deltaTime);
    return;
}
```

### Magic Numbers Extraction Pattern

```csharp
// 1. Add section at top of file
// ═══════════════════════════════════════════════════════════════════════
// MAGIC NUMBERS - TODO: Move to config
// ═══════════════════════════════════════════════════════════════════════
private const float SOME_THRESHOLD = 0.1f; // Descriptive comment

// 2. Use throughout file
if (value > SOME_THRESHOLD) { ... }
```

---

## Known Limitations / Future Work

### Current State

- ✅ All movement handlers implemented
- ✅ Animation system integrated
- ✅ Input system cleaner
- ✅ Config all in ScriptableObject
- ✅ Air control feels good

### Future Enhancements

- [ ] Move DefaultMovement magic numbers to config
- [ ] Move MantleHandler magic numbers to config
- [ ] Wall run handler (new feature)
- [ ] Grapple hook handler (new feature)
- [ ] Make HSM authoritative instead of observation-only

---

## Quick Reference

### Adding a New Handler

1. **Create Handler File** (e.g., `WallRunHandler.cs`)

   ```csharp
   public class WallRunHandler {
       private readonly KinematicCharacterMotor _motor;
       private readonly PlayerMovementConfig _config;

       public WallRunHandler(KinematicCharacterMotor motor, PlayerMovementConfig config) {
           _motor = motor;
           _config = config;
       }

       public bool CanWallRun() { /* detection logic */ }
       public void UpdateWallRun(ref Vector3 vel, float dt) { /* physics */ }
   }
   ```

2. **Add to DefaultMovement**

   ```csharp
   private readonly WallRunHandler _wallRunHandler;

   public DefaultMovement(...) {
       _wallRunHandler = new WallRunHandler(motor, config);
   }
   ```

3. **Integrate into UpdateVelocity**

   ```csharp
   if (_wallRunHandler.CanWallRun()) {
       _wallRunHandler.UpdateWallRun(ref currentVelocity, deltaTime);
       return; // or continue based on priority
   }
   ```

4. **Add Config Values**
   ```csharp
   [Header("Wall Run")]
   public float WallRunSpeed = 10f;
   public float WallRunDuration = 2f;
   ```

---

## Summary

**This system is cleaner because:**

1. **One Brain:** DefaultMovement coordinates everything
2. **No Priorities:** Explicit flow, readable code
3. **Pure Handlers:** Answer questions, modify state
4. **Clear Integration:** Input → Controller → Movement → Physics
5. **Configuration:** All values in ScriptableObject

**Key Files to Understand:**

1. `DefaultMovement.cs` - The coordinator
2. `JumpHandler.cs` - Reference implementation
3. `PlayerMovementConfig.cs` - All the numbers
4. `PlayerInputHandler.cs` - How input flows

**To Port to Another Project:**
Copy the pattern, not the code. Start with JumpHandler, then add others one by one.
