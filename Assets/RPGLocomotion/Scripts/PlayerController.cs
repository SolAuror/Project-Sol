using UnityEngine;

namespace Sol.FinalPlayerController
{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour
    {
#region Class Variables
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Transform _camTransform;
        [SerializeField] private bool _shouldFaceMoveDirection = false;
        public Camera PlayerCamera { get => _playerCamera; set => _playerCamera = value; }
        public Transform CamTransform { get => _camTransform; set => _camTransform = value; }
        public float rotationMismatch { get; private set; } = 0f;
        public bool IsRotatingToTarget { get; private set; } = false;
        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerState _playerState;

        [Header("Locomotion Settings")]
        public float movementThreshold = 0.01f;
        public float drag = 0.3f;
        public float gravity = 25f;
        public float terminalVelocity = 50f;


        [Header("Walk")]
        public float walkAcceleration = .15f;
        public float walkSpeed = 3f;


        [Header("Run(Default)")]
        public float runAcceleration = .3f;
        public float runSpeed = 6f;


        [Header("Sprint")]
        public float sprintAcceleration = .5f;
        public float sprintSpeed = 9f;

        [Header("Speed Smoothing")]
        [SerializeField] private float speedLerpFactor = 8f; // larger = faster convergence
        private float currentMaxSpeed = 0f;

        [Header("Crouch")]
        public float crouchAcceleration = .15f;
        public float crouchSpeed = 3f;
        private float _standingHeight;
        private Vector3 _standingCenter;
        private bool _isCrouching;
        [SerializeField] private float crouchHeight = 1.2f;
        [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.595f, 0);
        [SerializeField] private float crouchTransitionSpeed = 5f;

        
        [Header("Jumping & In-Air")]
        public float jumpSpeed = 1.0f;
        public float LedgeJumpCoyoteTime = 0.1f;
        public float inAirAcceleration = 0.15f;
        public float inAirDrag = 0.1f;
        
        [Header("Animation")]
        public float playerModelRotationSpeed = 10f;
        public float rotateToTargetTime = 0.67f;

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 70f;
        private Vector2 _cameraRotation = Vector2.zero;
        private Vector2 _playerTargetRotation = Vector2.zero;
        public Vector2 CameraRotation { get => _cameraRotation; set => _cameraRotation = value; }

        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayers;

        [Header("Slope/Wall handling")]
        [SerializeField] private float steepCheckDistance = 0.25f;
        [SerializeField] private float steepCheckDisableDuration = 0.12f;
        private float _steepWallDisableTimer = 0f;
        private bool _isNearWallCached = false;


        private bool _jumpedLastFrame = false;
        private bool _isRotatingClockwise = false;
        private float _rotatingToTargetTimer = 0f;
        private float _verticalVelocity = 0f;
        private float _antiBump;
        private float _stepOffset;
        private float _ledgeJumpCoyoteTimer = 0f;

        private PlayerMovementState _lastMovementState = PlayerMovementState.Falling;
#endregion



#region Initialize
        private void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();

            _antiBump = sprintSpeed;
            _stepOffset = _characterController.stepOffset;

            _standingHeight = _characterController.height;
            _standingCenter = _characterController.center;
            // initialize speed smoothing
            currentMaxSpeed = walkSpeed;
        }
#endregion



#region Update Logic
        private void Update()
        {
            HandleVerticalMovement(); //ground and jump checks
            UpdateMovementState(); //update movement states
            UpdateCrouchShape();//update crouch
            HandleLateralMovement(); //do movement
        }

        private void UpdateMovementState() 
        {
            _lastMovementState = _playerState.CurrentPlayerMovementState;

            bool canRun = CanRun();
            bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
            bool isMovingLaterally = IsMovingLaterally();
            bool isSprinting = _playerLocomotionInput.SprintPressed && isMovingLaterally;
            bool isWalking = isMovingLaterally && (!canRun || _playerLocomotionInput.WalkToggle);

            //possibly some redundancy here but it fixed the falling into crouuch bug
            bool isGrounded = IsGrounded();
            bool isTrulyGrounded = _playerState.InGroundedState() && isGrounded;

            bool wantsToCrouch = _playerLocomotionInput.CrouchToggle && isTrulyGrounded;

            // update steep-wall timer
            if (_steepWallDisableTimer > 0f)
                _steepWallDisableTimer -= Time.deltaTime;

            // Compute and cache wall detection once per update
            _isNearWallCached = PlayerControlUtil.DetectNearbyWall(_characterController, transform, _groundLayers, steepCheckDistance);

            if (_isNearWallCached)
                _steepWallDisableTimer = steepCheckDisableDuration;

            PlayerMovementState lateralState = wantsToCrouch ? PlayerMovementState.Crouching :
                                               isWalking ? PlayerMovementState.Walking :   
                                               isSprinting ? PlayerMovementState.Sprinting :
                                               isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idle;

            if (_playerState.CurrentPlayerMovementState == PlayerMovementState.Crouching && !wantsToCrouch)
            {
                if (CanStandUp()) _playerState.SetPlayerMovementState(lateralState);
                else _playerState.SetPlayerMovementState(PlayerMovementState.Crouching);
            }
            else
            {
                _playerState.SetPlayerMovementState(lateralState);
            }
            
            //control Airborne
            if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y > 0f)
            {
                _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            }

            else if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y <= 0f)
            {
                _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            }
            else
            {
                // respect steep-wall disable timer to avoid stepOffset on walls
                if (_steepWallDisableTimer > 0f)
                    _characterController.stepOffset = 0f;
                else
                    _characterController.stepOffset = _stepOffset;
            }
        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = _playerState.InGroundedState();

            _verticalVelocity -= gravity * Time.deltaTime;

            if (isGrounded && _verticalVelocity <= 0)
            {
                _verticalVelocity = -_antiBump;
                _ledgeJumpCoyoteTimer = LedgeJumpCoyoteTime;
            }
            else
            {
                _ledgeJumpCoyoteTimer -= Time.deltaTime;
            }


            if (_playerLocomotionInput.JumpPressed && (isGrounded || _ledgeJumpCoyoteTimer > 0f))
            {
                _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
                _jumpedLastFrame = true;
                _ledgeJumpCoyoteTimer = 0f;
            }

            if (_playerState.IsStateGroundedState(_lastMovementState) && !isGrounded)
            {
                _verticalVelocity += _antiBump;
            }

            if (Mathf.Abs(_verticalVelocity) > Mathf.Abs(terminalVelocity))
            {
                _verticalVelocity = -1f * Mathf.Abs(terminalVelocity);
            }

        }

        private void HandleLateralMovement()
        {

            bool isGrounded = _playerState.InGroundedState();
            bool isCrouching = _playerState.CurrentPlayerMovementState == PlayerMovementState.Crouching;
            bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

            //state dependant accel
            float lateralAcceleration = !isGrounded ? inAirAcceleration :
                                          isWalking ? walkAcceleration :
                                          isCrouching ? crouchAcceleration :
                                          isSprinting ? sprintAcceleration : runAcceleration;
            float targetSpeed = !isGrounded ? sprintSpeed :
                                          isWalking ? walkSpeed :
                                          isCrouching ? crouchSpeed :
                                          isSprinting ? sprintSpeed : runSpeed;

            // speed smoothing
            currentMaxSpeed = Mathf.Lerp(currentMaxSpeed, targetSpeed, speedLerpFactor * Time.deltaTime);
            float clampLateralMagnitude = currentMaxSpeed;

            // Sets movement to camera direction
            Vector3 forward = new Vector3(_camTransform.forward.x, 0f, _camTransform.forward.z).normalized;
            Vector3 right = new Vector3(_camTransform.right.x, 0f, _camTransform.right.z).normalized;
            Vector3 movementDirection = right * _playerLocomotionInput.MovementInput.x + forward * _playerLocomotionInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
            Vector3 combinedVelocity = _characterController.velocity + movementDelta;

            if (_shouldFaceMoveDirection && movementDirection.sqrMagnitude > movementThreshold)
            {
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
            }

            //Apply drag to character, if new drag is greater than current, subtract current or set to 0
            float dragMagnitude = isGrounded ? drag : inAirDrag;
            Vector3 currentDrag = combinedVelocity.normalized * dragMagnitude * Time.deltaTime;
            combinedVelocity = (combinedVelocity.magnitude > dragMagnitude * Time.deltaTime) ? combinedVelocity - currentDrag : Vector3.zero;
            combinedVelocity = Vector3.ClampMagnitude(new Vector3(combinedVelocity.x, 0f, combinedVelocity.z), clampLateralMagnitude);
            combinedVelocity.y += _verticalVelocity;
            if (!isGrounded)
            {
                // Adjust for steep ground normals (sliding) using utility
                combinedVelocity = PlayerControlUtil.AdjustVelocityForSteepGround(_characterController, combinedVelocity, _groundLayers, 0.5f);
            }

            _characterController.Move(combinedVelocity * Time.deltaTime);
        }

        private void UpdateCrouchShape()
        {
            float targetHeight = _playerState.CurrentPlayerMovementState == PlayerMovementState.Crouching ? crouchHeight : _standingHeight;
            Vector3 targetCenter = _playerState.CurrentPlayerMovementState == PlayerMovementState.Crouching ? crouchCenter : _standingCenter;

            _characterController.height = Mathf.Lerp(_characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            _characterController.center = Vector3.Lerp(_characterController.center, targetCenter, Time.deltaTime * crouchTransitionSpeed);
        }
#endregion



#region Late Update Logic
        private void LateUpdate()
        {
            UpdateCameraRotation();
        }
        private void UpdateCameraRotation()
        {
            _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

            _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;

            float rotationTolerance = 90f;
            bool isIdle = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idle;
            IsRotatingToTarget = _rotatingToTargetTimer > 0;

            //also rotate if not idle
            if (!isIdle)
            {
                RotatePlayerToTarget();
            }
            //if rot mismatch is not in tolerance or timer is active, then rotate
            else if (Mathf.Abs(rotationMismatch) > rotationTolerance || IsRotatingToTarget)
            {
                UpdateIdleRotation(rotationTolerance);
            }

            //fetch angle between the camera and player
            Vector3 camForwardProjectedXZ = new Vector3(_camTransform.forward.x, 0f, _camTransform.forward.z).normalized;
            Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
            rotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);
        }

        private void UpdateIdleRotation(float rotationTolerance)
        {
            //initiate new rot direction
            if (Mathf.Abs(rotationMismatch) > rotationTolerance)
            {
                _rotatingToTargetTimer = rotateToTargetTime;
                _isRotatingClockwise = rotationMismatch > rotationTolerance;
            }
            _rotatingToTargetTimer -= Time.deltaTime;

            //rotate player
            if (_isRotatingClockwise && rotationMismatch > 0f ||
                    !_isRotatingClockwise && rotationMismatch < 0f)
            {
                RotatePlayerToTarget();
            }

        }
        
        private void RotatePlayerToTarget()
        { 
            Vector3 camForwardProjectedXZ = new Vector3(_camTransform.forward.x, 0f, _camTransform.forward.z).normalized;

            if (camForwardProjectedXZ.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(camForwardProjectedXZ, Vector3.up);

            // if rotating to target (timer > 0) prefer timer-driven interpolation so the rotation
            // completes within the remaining _rotatingToTargetTimer; otherwise use regular speed.
            float regularRotSpeed;
            if (_rotatingToTargetTimer > 0f && rotateToTargetTime > 0f)
            {
                // interpolate remaining time so the rotation becomes faster as zero approaches.
                regularRotSpeed = Mathf.Clamp01(Time.deltaTime / Mathf.Max(0.0001f, _rotatingToTargetTimer));
            }
            else
            {
                regularRotSpeed = playerModelRotationSpeed * Time.deltaTime;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Clamp01(regularRotSpeed));

            // snap to target if close to avoid jitter.
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
                transform.rotation = targetRotation;
        }
#endregion



#region State Checks
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);

            return lateralVelocity.magnitude > movementThreshold;
        }

        private bool IsGrounded() 
        {
            bool grounded = _playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();

            return grounded;
        }

        private bool IsGroundedWhileGrounded()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _characterController.radius, transform.position.z);

            bool grounded = Physics.CheckSphere(spherePosition, _characterController.radius, _groundLayers, QueryTriggerInteraction.Ignore);

            return grounded;
        }

        private bool IsGroundedWhileAirborne()
        {
            Vector3 normal = PlayerControlUtil.GetNormalWithSphereCast(_characterController, _groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= _characterController.slopeLimit;

            return _characterController.isGrounded && validAngle;
        }


        //must be moving forward at 45 degrees or less on the x, otherwise cant run
        private bool CanRun()
        {
            return _playerLocomotionInput.MovementInput.y >= Mathf.Abs(_playerLocomotionInput.MovementInput.x);
        }

        private bool CanStandUp()
        {
            float headRadius = Mathf.Max(0.05f, _characterController.radius * 0.9f);
            Vector3 start = transform.position + _characterController.center + Vector3.up * (_characterController.height * 0.5f);
            Vector3 end = transform.position + _standingCenter + Vector3.up * (_standingHeight * 0.5f);
            return !Physics.CheckCapsule(start, end, headRadius, _groundLayers, QueryTriggerInteraction.Ignore);
        }
#endregion
    }
}
    

