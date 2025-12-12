# MiniArch Integration Guide for AI Agent

**Purpose:** This guide helps you (the AI agent) understand how to port the "handler-based composition" movement system from this project to another Unity project with a priority-based module system.

---

## Quick Context

**What This Is:**
A complete movement system using handler-based composition where one coordinator (`DefaultMovement`) delegates to specialized handlers (`JumpHandler`, `SlideHandler`, etc.) instead of priority-based modules fighting for control.

**Why It's Better:**

- No priority wars
- Clear execution flow
- Easy to debug
- Scalable

---

## Package Contents

This `MiniArch` folder contains markdown files with the complete source code of:

### Core Files

1. **DefaultMovement.md** - The brain/coordinator
2. **PlayerController.md** - Entry point
3. **PlayerInputHandler.md** - Input storage pattern
4. **PlayerMovementConfig.md** - ScriptableObject configuration

### Handler Files

5. **JumpHandler.md** - All jump logic
6. **SlideHandler.md** - Slide physics
7. **DashHandler.md** - Dash system
8. **MantleHandler.md** - Mantle/shimmy logic

### Supporting Files

9. **MovementModuleBase.md** - Abstract base
10. **PlayerMovementSystem.md** - Module manager
11. **IMovementModule.md** - Interface
12. **CameraInputProcessor.md** - Camera-relative input
13. **CinemachineInputBridge.md** - Camera integration

---

## Integration Strategy

### Phase 1: Understand The Pattern (30 min)

Read these files in order to understand the architecture:

1. **DefaultMovement.md** - See how one coordinator owns the flow
2. **JumpHandler.md** - See how handlers answer questions
3. **PlayerMovementConfig.md** - See configuration pattern

**Key Insight:**

```csharp
// OLD (Priority-based):
if (module.Priority > other.Priority && module.CanEnterState())
    module.UpdatePhysics();

// NEW (Handler-based):
if (_jumpHandler.CanJump())
    _jumpHandler.ProcessJump(ref velocity, deltaTime, input);
```

### Phase 2: Extract Handlers from Modules (2-3 hours)

For each module in the target project:

**Example: Converting AirborneModule to JumpHandler**

1. **Identify pure logic** (no state machine checks)

   - Jump detection
   - Jump execution
   - Physics calculations

2. **Create new Handler class**

   ```csharp
   public class JumpHandler {
       private readonly KinematicCharacterMotor _motor;
       private readonly PlayerMovementConfig _config;

       public JumpHandler(KinematicCharacterMotor motor, PlayerMovementConfig config) {
           _motor = motor;
           _config = config;
       }
   }
   ```

3. **Extract methods** from module to handler

   ```csharp
   // OLD: AirborneModule.TryGroundJump()
   // NEW: JumpHandler.TryGroundJump()

   // Keep the LOGIC, remove the CONTROL FLOW
   ```

4. **Make it queryable** (not controlling)
   ```csharp
   // Handler answers questions
   public bool CanJump() { /* detection */ }
   public void ProcessJump(ref Vector3 vel, float dt, Vector3 input) { /* physics */ }
   ```

**Repeat for:**

- SlideModule → SlideHandler
- WallRunModule → WallRunHandler (if exists)
- etc.

### Phase 3: Build The Coordinator (2-3 hours)

Create a new `DefaultMovement.cs` (use **DefaultMovement.md** as template):

**Core Pattern:**

```csharp
public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
{
    // 1. Special states (full override)
    if (_mantleHandler.IsActive) {
        _mantleHandler.UpdateMantle(ref currentVelocity, deltaTime);
        return;
    }

    // 2. Ground vs Air
    if (Motor.GroundingStatus.IsStableOnGround)
        ApplyGroundMovement(ref currentVelocity, deltaTime);
    else
        ApplyAirMovement(ref currentVelocity, deltaTime);

    // 3. Jump processing (works in both)
    _jumpHandler.ProcessJump(ref currentVelocity, deltaTime, _moveInputVector);

    // 4. Dash/impulses
    ApplyInternalVelocity(ref currentVelocity);
}
```

**Key Methods to Implement:**

- `ApplyGroundMovement()` - Calls SlideHandler if sliding, else normal movement
- `ApplyAirMovement()` - Air control + dash
- `HandleCrouch()` - Capsule resizing

### Phase 4: Wire Input System (1 hour)

Adapt **PlayerInputHandler.md** pattern to your input system:

**Core Principle:** Store, don't execute

```csharp
// BAD (executes immediately)
if (Input.GetKeyDown(KeyCode.Space)) {
    character.Jump();
}

// GOOD (stores intent)
if (Input.GetKeyDown(KeyCode.Space)) {
    _jumpRequested = true; // Consumed in physics update
}
```

**Properties to Expose:**

```csharp
public Vector2 MoveInput { get; }
public bool JumpDown { get; }  // One-frame trigger
public bool JumpHeld { get; }  // Continuous state
public bool IsSprinting { get; } // Toggle/hold support
public bool DashJustActivated { get; }
```

### Phase 5: Port Configuration (1 hour)

Create **PlayerMovementConfig** ScriptableObject from **PlayerMovementConfig.md**:

**Template:**

```csharp
[CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "YourProject/Movement Config")]
public class PlayerMovementConfig : ScriptableObject
{
    [Header("Ground Movement")]
    public float MaxStableMoveSpeed = 8f;
    public float StableMovementSharpness = 15f;

    [Header("Jumping")]
    public float JumpSpeed = 10f;
    public float JumpScalableForwardSpeed = 10f;
    // etc...
}
```

**Benefits:**

- All numbers in one place
- Inspector-editable
- Create presets (Fast/Slow variants)

### Phase 6: Test Integration (1-2 hours)

**Test Order:**

1. Ground movement (walk/run)
2. Jump (ground, coyote, double)
3. Air control (strafe)
4. Dash (ground + air)
5. Slide (if applicable)
6. Mantle (if applicable)

**Debug Pattern:**

```csharp
// Add debug logs to handlers
if (DEBUG_MODE)
    Debug.Log($"[JumpHandler] Executing jump: type={type}, upSpeed={upSpeed}");
```

---

## Critical Files to Understand

### 1. DefaultMovement.md - The Brain

**What to copy:**

- `UpdateVelocity()` flow pattern
- Handler composition (not inheritance)
- Clear execution order

**What NOT to copy:**

- Specific KCC integration (might differ)
- Magic number values (tune for your game)

**Key Section: MAGIC NUMBERS**

```csharp
// ═══════════════════════════════════════════════════════════════════════
// MAGIC NUMBERS - TODO: Move to config
// ═══════════════════════════════════════════════════════════════════════
private const float DASH_VELOCITY_THRESHOLD = 0.1f;
private const float AIR_SPEED_DECAY_SHARPNESS = 10f;
```

→ Extract these to config first!

### 2. JumpHandler.md - Reference Implementation

**What to copy:**

- Handler constructor pattern
- Public API design (RequestJump, ProcessJump, CanJump)
- Multi-jump counter system
- Scalable jump velocity (forward momentum)

**Key Physics:**

```csharp
// Vertical impulse
currentVelocity += (jumpDirection * upSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);

// Forward impulse (Titanfall-style)
if (moveInputVector.sqrMagnitude > 0f) {
    currentVelocity += moveInputVector * forwardSpeed;
}
```

**Different jump types use different speeds:**

```csharp
float upSpeed = (type == JumpType.Air) ? _config.DoubleJumpSpeed : _config.JumpSpeed;
float forwardSpeed = (type == JumpType.Air) ? _config.DoubleJumpScalableForwardSpeed : _config.JumpScalableForwardSpeed;
```

### 3. SlideHandler.md - Momentum Physics

**What to copy:**

- Momentum-based decay model
- Friction curve pattern
- Slope influence calculation

**Key Algorithm:**

```csharp
// 1. Capture initial speed with boost
_initialSlideSpeed = currentSpeed * _config.SlideSpeedBoost;

// 2. Apply friction curve
float slideProgress = _slideTimer / _config.MaxSlideDuration;
float frictionFactor = _config.SlideFrictionCurve.Evaluate(slideProgress);
float currentSlideSpeed = _initialSlideSpeed * frictionFactor;

// 3. Slope influence
float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
float slopeFactor = (slopeAngle - 90f) / 90f; // -1 to +1
currentSlideSpeed += slopeFactor * _config.SlopeInfluence;
```

### 4. DashHandler.md - Charge System

**What to copy:**

- Multi-charge pattern
- Reload timer
- Intermission cooldown

**CRITICAL: Universal Dash Distance**

The current code has a commented-out section for consistent dash distance. If you want dashes to travel the same distance regardless of velocity:

```csharp
// Cancel existing velocity in dash direction
Vector3 dashVelocityToCancel = Vector3.Project(velocityAdd, direction);
velocityAdd -= dashVelocityToCancel;

// Then apply full force
velocityAdd += direction * _config.DashForce;
```

Without this, dash is additive (current code).

### 5. MantleHandler.md - Complex State Machine

**What to copy IF you need mantle:**

- Raycast-based detection (no trigger colliders)
- State machine pattern (None → Grabbing → Hanging → Mantling)
- Arc-based motion

**What to skip if not needed:**

- Shimmy logic (can add later)
- Drop logic (can add later)

**MAGIC NUMBERS (move to config):**

```csharp
private const float SHIMMY_CHECK_VERTICAL = 0.3f;
private const float SHIMMY_CHECK_FORWARD = 0.5f;
private const float SHIMMY_SPHERE_RADIUS = 0.3f;
private const float GRAB_PULLBACK = 0.05f;
private const float MANTLE_FORWARD_OFFSET = 0.15f;
private const float DROP_COOLDOWN = 0.5f;
```

---

## Common Pitfalls

### 1. Don't Port The Priority System

**BAD:**

```csharp
// Don't do this - defeats the purpose!
if (_jumpHandler.Priority > _slideHandler.Priority)
    _jumpHandler.ProcessJump();
```

**GOOD:**

```csharp
// Explicit flow
if (grounded) ApplyGroundMovement();
else ApplyAirMovement();
_jumpHandler.ProcessJump(); // Works in both states
```

### 2. Don't Make Handlers Control Flow

**BAD:**

```csharp
public class JumpHandler {
    public void Activate() {
        ExecuteJump();
        TransitionToAirState(); // ❌ Handler shouldn't control state
    }
}
```

**GOOD:**

```csharp
public class JumpHandler {
    public void ProcessJump(ref Vector3 vel, float dt, Vector3 input) {
        if (!_jumpRequested) return;
        ExecuteJump(ref vel, input);
        _jumpRequested = false;
    }
}
```

### 3. Don't Skip Configuration Pattern

**BAD:**

```csharp
public class JumpHandler {
    private const float JUMP_SPEED = 10f; // Hardcoded!
}
```

**GOOD:**

```csharp
public class JumpHandler {
    private readonly PlayerMovementConfig _config;

    public void ExecuteJump(...) {
        float jumpSpeed = _config.JumpSpeed; // Configurable!
    }
}
```

---

## File-by-File Porting Checklist

Use this to track your progress:

### Handlers

- [ ] Copy `JumpHandler.md` → Create `JumpHandler.cs` in target
- [ ] Copy `SlideHandler.md` → Create `SlideHandler.cs` (if needed)
- [ ] Copy `DashHandler.md` → Create `DashHandler.cs`
- [ ] Copy `MantleHandler.md` → Create `MantleHandler.cs` (if needed)

### Core Files

- [ ] Copy `DefaultMovement.md` → Create new `DefaultMovement.cs`
- [ ] Copy `MovementModuleBase.md` → Keep or adapt
- [ ] Copy `IMovementModule.md` → Keep or adapt

### Input System

- [ ] Read `PlayerInputHandler.md` → Adapt to your input system
- [ ] Implement "store, don't execute" pattern
- [ ] Add toggle/hold support for sprint/crouch

### Configuration

- [ ] Copy `PlayerMovementConfig.md` → Create ScriptableObject
- [ ] Extract all magic numbers from handlers
- [ ] Create config asset in Unity

### Controller

- [ ] Read `PlayerController.md` → Adapt your entry point
- [ ] Wire input → movement system
- [ ] Remove old priority-based logic

---

## Testing Checklist

After porting each handler, test:

### JumpHandler

- [ ] Ground jump works
- [ ] Coyote time works (jump after leaving ledge)
- [ ] Double jump works
- [ ] Jump buffer works (pre-landing)
- [ ] Forward momentum scales with input
- [ ] Wall jump works (if applicable)

### SlideHandler (if ported)

- [ ] Slide entry requires min speed
- [ ] Speed boost on entry
- [ ] Friction decay over time
- [ ] Slope affects speed (downhill faster)
- [ ] Auto-exit on min speed
- [ ] Steering works

### DashHandler

- [ ] Dash consumes charge
- [ ] Charges reload over time
- [ ] Intermission prevents spam
- [ ] Ground dash uses velocity direction
- [ ] Air dash uses input direction

### Integration

- [ ] All handlers work together
- [ ] No priority conflicts
- [ ] Config changes take effect
- [ ] Debug logs are clean

---

## Debugging Guide

### Handler Not Working

1. **Check constructor** - Is handler receiving Motor and Config?
2. **Check input flow** - Is RequestX() being called?
3. **Check processing** - Is ProcessX() being called in physics update?
4. **Add debug logs**:
   ```csharp
   Debug.Log($"[{GetType().Name}] State check: requested={_requested}, canDo={CanDo()}");
   ```

### Unexpected Behavior

1. **Check execution order** - Is handler called at right time in UpdateVelocity?
2. **Check magic numbers** - Are constants too sensitive?
3. **Check config values** - Are they reasonable for your game?

### Performance Issues

1. **Check raycast frequency** - MantleHandler uses raycasts, optimize with layers
2. **Check overlap tests** - Crouch uses CharacterOverlap, only when needed
3. **Profile handlers** - Use Unity Profiler to find bottlenecks

---

## Quick Start (Minimal Port)

If you only want the essentials:

**Minimum Viable Port:**

1. `JumpHandler` - Most games need jumping
2. `DefaultMovement` (simplified) - Just ground + air
3. `PlayerMovementConfig` - Config pattern
4. `PlayerInputHandler` pattern - Fixed input flow

**Skip for now:**

- SlideHandler (add later)
- MantleHandler (add later)
- DashHandler (add later)

**Time Estimate:** 3-4 hours for minimal port

---

## Next Steps After Port

1. **Test thoroughly** - Use checklist above
2. **Tune values** - Adjust config for your game feel
3. **Add features** - Wall run, grapple, etc.
4. **Refactor** - Move magic numbers to config
5. **Document** - Update this guide with your learnings

---

## Questions An AI Agent Might Ask

**Q: Do I need to port the entire file structure?**  
A: No! Only port the handlers you need. JumpHandler is most important. Slide/Mantle are optional.

**Q: Can I keep my existing input system?**  
A: Yes! Just adopt the "store, don't execute" pattern from PlayerInputHandler.md.

**Q: What if my KCC integration is different?**  
A: The handler pattern is KCC-agnostic. Just adapt the Motor calls to your framework.

**Q: Should I delete the old priority-based modules?**  
A: Not yet! Keep them until new system is tested. Then delete.

**Q: How do I handle wall running / grappling?**  
A: Create new handlers following the JumpHandler pattern. Add to DefaultMovement.UpdateVelocity().

**Q: The values feel wrong in my game!**  
A: Tune the config! Every game is different. Use the ScriptableObject pattern to test presets.

---

## Contact / Handoff

If you (the AI agent) get stuck:

1. **Re-read the pattern files** - DefaultMovement.md, JumpHandler.md
2. **Check system_architecture.md** - Available in project docs
3. **Look for MAGIC NUMBERS sections** - These need config values
4. **Test incrementally** - Port one handler at a time

**Success Criteria:**

- No more priority checks
- Clear execution flow in DefaultMovement
- All handlers are queryable, not controlling
- Config pattern used everywhere

Good luck with the migration! 🚀
