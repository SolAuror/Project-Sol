Sol Player Controller — README
==============================

How the Sol locomotion system differs from Unity's default Character Controller
--------------------------------------------------------------------------------

Unity's "default" character controller setup (as seen in the Starter Assets –
Third Person Character Controller package) is intentionally minimal: one script,
one speed, simple gravity, and a direct call to CharacterController.Move().
The Sol system is built on the same CharacterController component but adds a
layer of RPG-grade features on top of it. The differences are explained below.


1. MODULAR, MULTI-SCRIPT ARCHITECTURE
   Unity default: 1–2 monolithic scripts handle everything.
   Sol: Responsibilities are split across 8 focused components, each run in a
   defined execution order via [DefaultExecutionOrder]:
     -3  PlayerInputManager  – singleton; creates & owns the Input Actions asset
     -2  PlayerLocomotionInput / ThirdPersonInput / PlayerActionsInput  – read raw input
     -1  PlayerController  – applies physics & movement each frame
      0  PlayerAnimation / PlayerState  – react to the final state

2. NAMED MOVEMENT STATES (PlayerState / PlayerMovementState enum)
   Unity default: uses booleans like isGrounded / isJumping inline.
   Sol: a dedicated PlayerState component holds the authoritative
   CurrentPlayerMovementState (Idle, Walking, Running, Sprinting, Jumping,
   Falling, Crouching, Swimming). Every other component reads this enum instead
   of each doing its own ground check, making state logic consistent.

3. FOUR GROUND SPEEDS + PER-STATE ACCELERATION
   Unity default: one walk speed and one sprint speed, toggled instantly.
   Sol: Walk, Run (default), Sprint, and Crouch each have their own
   acceleration and top speed values. Air movement uses inAirAcceleration.
   A Lerp (speedLerpFactor) smoothly blends currentMaxSpeed toward the target
   speed so transitions feel organic rather than snapping.

4. CUSTOM GRAVITY & TERMINAL VELOCITY
   Unity default: multiplies Physics.gravity.y by a constant each frame.
   Sol: applies its own configurable gravity (25 f/s²) via:
       _verticalVelocity -= gravity * Time.deltaTime
   and clamps the result at terminalVelocity, giving designers full control
   without touching Project Settings.

5. ANTI-BUMP GROUNDING PUSH
   Unity default: does not address the "stairs pop" artifact explicitly.
   Sol: stores _antiBump = sprintSpeed and applies -_antiBump as the resting
   vertical velocity when grounded. This keeps the character pressed firmly
   against ramps and stairs instead of bunny-hopping over them.

6. COYOTE TIME (Ledge-Jump Buffer)
   Unity default: jump requires isGrounded == true at the exact frame of input.
   Sol: a LedgeJumpCoyoteTime timer (default 0.1 s) keeps the jump available
   briefly after the character walks off a ledge, matching the feel of
   platform games where "I was just on that ledge" is still forgiven.

7. PHYSICS-BASED JUMP IMPULSE FORMULA
   Unity default: adds a fixed impulse value.
   Sol: derives the launch velocity from the jump height and gravity:
       _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity)
   Changing gravity or jumpSpeed automatically produces a consistent arc.

8. DIRECTIONAL DRAG (not Physics drag)
   Unity default: zeros lateral velocity when there is no input.
   Sol: subtracts a drag vector each frame (separate drag values for grounded
   and in-air), so the character decelerates realistically over a short distance
   instead of stopping instantly. Drag is applied after adding new input, so
   player control always works against friction.

9. DUAL GROUNDED-CHECK STRATEGY
   Unity default: relies solely on CharacterController.isGrounded.
   Sol: uses two separate checks depending on the current state:
     • While grounded   → Physics.CheckSphere at foot level (catches flat ground
                          and shallow steps cleanly).
     • While airborne   → SphereCast + slope-angle validation so the controller
                          only re-lands on slopes within the configured slopeLimit,
                          preventing false-positive grounding on steep geometry.

10. SLOPE & STEEP-WALL HANDLING (PlayerControlUtil)
    Unity default: built-in slope limit pushes the capsule away but can still
    let the character "walk up" steep inclines or jitter against walls.
    Sol: PlayerControlUtil adds two utilities on top:
     • AdjustVelocityForSteepGround – projects velocity onto the steep surface
       normal (with a friction multiplier) when airborne, so the character slides
       naturally instead of floating.
     • DetectNearbyWall – spherecasts in 4 horizontal directions; if a wall
       steeper than slopeLimit is detected, a steepWallDisableTimer fires and
       stepOffset is set to 0 for that duration, preventing the character from
       being pushed up the wall by the step-climbing logic.

11. DYNAMIC STEP OFFSET
    Unity default: stepOffset is a fixed value set in the inspector.
    Sol: disables stepOffset (sets it to 0) while the character is airborne or
    near a steep wall, then restores it only when firmly grounded on walkable
    terrain. This prevents the CharacterController from "climbing" vertical
    walls via its step logic.

12. SMOOTH CROUCH (capsule resize via Lerp)
    Unity default: no crouch system.
    Sol: UpdateCrouchShape() Lerps both the CharacterController height and center
    toward configurable crouchHeight/crouchCenter each frame, producing a smooth
    crouch-stand animation. CanStandUp() uses CheckCapsule to block standing if
    there is an obstacle above the player's head.

13. FORWARD-ONLY SPRINT RESTRICTION
    Unity default: allows sprinting in any direction.
    Sol: CanRun() returns true only when MovementInput.y >= |MovementInput.x|,
    meaning the player must be moving mostly forward to reach run/sprint speed.
    This is typical of RPGs where backward or side movement is capped at walk.

14. IDLE ROTATION SNAPPING
    Unity default: the character body always faces the movement direction or is
    locked to a fixed forward.
    Sol: while Idle the system measures rotationMismatch (signed angle between
    player forward and camera forward). When the mismatch exceeds 90° a timed
    rotation sequence fires (_rotatingToTargetTimer), smoothly snapping the
    player to face the camera. The animator is driven by both rotationMismatch
    and isRotatingToTarget for turn-in-place animations.

15. FIRST / THIRD PERSON CAMERA SWITCHING
    Unity default: ships as either first or third person — not both.
    Sol: PlayerInputManager toggles between two Cinemachine cameras at runtime,
    preserving the current look rotation. During first-person mode the head mesh
    is set to ShadowsOnly to avoid clipping; it is restored when switching back.
    A configurable blend delay (_perspectiveSwapDelay) prevents rapid toggling
    during the Cinemachine blend.

16. ORBITAL CAMERA ZOOM (ThirdPersonInput)
    Unity default: no zoom.
    Sol: ThirdPersonInput reads mouse scroll wheel and gamepad D-pad to adjust
    CinemachineOrbitalFollow.Radius within [_cameraMinZoom, _cameraMaxZoom].
    The radius change is smoothed with a Lerp (_zoomLerpSpeed) so the zoom feels
    weighted rather than snapping.

17. SEPARATE ACTION INPUT LAYER (PlayerActionsInput)
    Unity default: no combat or interaction inputs.
    Sol: PlayerActionsInput tracks Attack, Aim, and Interact presses and exposes
    them to PlayerAnimation. InteractPressed is automatically cleared when the
    player moves, jumps, falls, or sprints, preventing stale input carry-over.

18. SINGLETON INPUT MANAGER WITH DontDestroyOnLoad
    Unity default: input lives on the player GameObject and is destroyed between
    scenes.
    Sol: PlayerInputManager is a singleton (Instance pattern) that calls
    DontDestroyOnLoad, so input bindings and camera state persist across scene
    loads without re-initialization.

19. ANIMATOR DRIVEN BY BLEND TREE MAGNITUDES
    Unity default: typically sets a single Speed float.
    Sol: PlayerAnimation sets inputX, inputY, and inputMagnitude floats that are
    Lerped (locomotionBlendSpeed) from the raw input each frame, enabling a 2-D
    blend tree for directional strafing animations. The maximum blend value varies
    per state (0.5 crouch → 0.75 walk → 1.0 run → 1.5 sprint) so the animator
    automatically plays the right tier of motion clip. 