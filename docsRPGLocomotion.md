# Player Movement, Input, and Camera System

This document explains the **RPGLocomotion system under Assets/RPGLocomotion**.

---

## 1. High-Level Overview

The system is split into **seven cooperating modules**:

1. **PlayerInputManager** – Owns input bindings and perspective switching
2. **PlayerActionsInput** – Handles combat / interaction actions
3. **PlayerLocomotionInput** – Handles movement-related inputs
4. **PlayerController** – Executes movement physics and rotation
5. **PlayerState** – Central authority on current movement state
6. **ThirdPersonInput** – Handles camera zoom for third-person view
7. **PlayerAnimation** – Translates state + input into animation parameters

All systems rely on Unity's **Player Input System**, **CharacterController**, and **Cinemachine**.

---

## 2. Input Architecture

### 2.1 PlayerInputManager (Global Singleton)

**Responsibilities**:
- Owns the `PlayerControls` input asset
- Enables/disables input maps
- Handles first-person / third-person camera swapping
- Manages perspective transition timing

**Lifecycle**:
- Enforces singleton in `Awake`
- Locks cursor in `Start`
- Initializes input bindings in `OnEnable`
- Cleans up callbacks in `OnDisable`

**Perspective Flow**:
1. Perspective toggle input flips `_isFirstPerson`
2. `SetPerspective` activates the appropriate Cinemachine camera
3. Camera priority is adjusted (active = 10, inactive = 0)
4. Player camera transform is reassigned
5. Head mesh shadow casting is adjusted
6. Coroutine waits for blend completion before clearing `_isTransitioning`

---

### 2.2 PlayerLocomotionInput (Movement Inputs)

**Captured Inputs**:
- Move (Vector2)
- Look (Vector2)
- Jump (press)
- Sprint (hold or toggle)
- Walk (toggle)
- Crouch (toggle)

**Key Behaviors**:
- Jump is **edge-triggered** (reset every `LateUpdate`)
- Sprint behavior supports both hold and toggle modes
- Jump cancels crouch

**Responsibility Boundary**:
This class **does not move the character**. It only records player intent.

---

### 2.3 PlayerActionsInput (Combat / Interaction)

**Captured Actions**:
- Attack
- Aim
- Interact
- Perspective swap (forwarded to PlayerInputManager)

**Special Rules**:
- `InteractPressed` is forcibly reset if the player starts moving, jumping, sprinting, or falling
- Aim toggling logic depends on whether the player is already aiming

---

## 3. Movement State System

### 3.1 PlayerState

Acts as the **single source of truth** for movement state.

**Movement States**:
- Idle
- Walking
- Running
- Sprinting
- Crouching
- Jumping
- Falling

**Key Functions**:
- `SetPlayerMovementState(state)`
- `InGroundedState()`
- `IsStateGroundedState(state)`

Grounded states are:
`Idle, Walking, Running, Sprinting, Crouching`

---

## 4. PlayerController (Locomotion Engine)

This is the **core physics and movement executor**.

### 4.1 Initialization

Cached data includes:
- CharacterController reference
- Camera and camera transform
- Movement parameters (acceleration, drag, gravity, terminal velocity)
- Jump and air-control values (coyote time, in-air acceleration)
- Crouch geometry (height, center, transition speed)
- Rotation and look parameters
- Ground, slope, and wall detection settings

---

### 4.2 Per-Frame Locomotion Loop

Each frame:

1. Read input from `PlayerLocomotionInput`
2. Determine desired movement state
3. Compute max speed for the state
4. Smooth speed transitions
5. Apply acceleration and drag
6. Apply gravity and clamp vertical velocity
7. Handle jump using coyote time
8. Transition to falling when airborne
9. Lerp crouch height and center
10. Detect steep slopes and walls
11. Project velocity when slope is too steep
12. Move the CharacterController
13. Rotate player toward movement or camera
14. Track rotation mismatch flags

**Important**:
- Physics is velocity-based, not force-based
- Gravity is applied manually
- Slopes are handled via surface normal projection

---

## 5. Camera Systems

### 5.1 ThirdPersonInput

**Responsibilities**:
- Adjust third-person camera zoom

**Inputs**:
- Mouse scroll wheel
- Gamepad D-pad

**Behavior**:
- Zoom input modifies a target radius
- Actual camera radius lerps smoothly
- Inputs are cleared in `LateUpdate`

---

### 5.2 Perspective Switching

- Uses Cinemachine camera priority swapping
- Preserves camera rotation across transitions
- Prevents logic conflicts using `_isTransitioning`

---

## 6. Environment Utilities

### PlayerControlUtil

Provides reusable physics helpers:

- **GetNormalWithSphereCast** – Ground normal detection
- **DetectNearbyWall** – Prevents pushing into steep walls
- **AdjustVelocityForSteepGround** – Slides player down steep slopes

These utilities are shared by the controller to keep movement logic clean.

---

## 7. Animation System

### PlayerAnimation

Bridges **gameplay state → Animator parameters**.

**Inputs**:
- PlayerState
- PlayerLocomotionInput
- PlayerActionsInput
- PlayerController rotation data

**Behavior**:
- Sets grounded, jump, fall, crouch flags
- Computes smoothed blend values from movement input
- Detects whether action animations are playing
- Optionally syncs animation with camera rotation mismatch

---

## 8. Common Runtime Risks & Pitfalls

### 8.1 Double Callback Registration

**Risk**:
- Multiple `AddCallbacks(this)` calls without matching removals

**Symptoms**:
- Inputs firing twice
- Toggle inputs flipping back immediately

**Mitigation**:
- Ensure callbacks are added only once per enable
- Defensive checks or centralized input binding

---

### 8.2 State Desynchronization

**Risk**:
- Input intent, PlayerState, and Animator flags diverge

**Examples**:
- Falling animation while grounded
- Sprint animation without sprint speed

**Mitigation**:
- Treat `PlayerState` as authoritative
- Avoid animator-driven gameplay logic

---

### 8.3 Edge-Triggered Input Loss

**Risk**:
- Jump or interact reset before being consumed

**Cause**:
- Clearing inputs in `LateUpdate`

**Mitigation**:
- Consume inputs exactly once per frame
- Avoid multiple systems reading the same flag

---

### 8.4 Camera & Rotation Mismatch

**Risk**:
- Player model snaps or rotates incorrectly during perspective swaps

**Cause**:
- Camera transform reassignment mid-frame

**Mitigation**:
- Gate rotation logic with `_isTransitioning`
- Preserve and restore rotations carefully

---

### 8.5 Slope Handling Edge Cases

**Risk**:
- Velocity popping when transitioning on/off steep slopes

**Mitigation**:
- Smooth normal transitions
- Avoid abrupt projection changes

---

## 9. Design Intent Summary

- **Input is declarative** (intent only)
- **State is centralized**
- **Movement is deterministic and physics-driven**
- **Camera and animation react to state, not vice versa**

This separation keeps the system flexible, debuggable, and extensible.

---

If you'd like, this can be extended with:
- A sequence diagram
- A runtime data-flow diagram
- A simplified "designer-facing" version
- Refactoring recommendations

