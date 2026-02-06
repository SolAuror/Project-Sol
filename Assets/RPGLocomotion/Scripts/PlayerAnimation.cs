using System.Linq;
using UnityEngine;

namespace Sol.FinalPlayerController
{
    public class PlayerAnimation : MonoBehaviour
    {
#region Class Variables
        [SerializeField] private Animator _animator;
        [SerializeField] private float locomotionBlendSpeed = 4f;
        private PlayerState _playerState;
        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerActionsInput _playerActionsInput;
        private PlayerController _playerController;

        //Locomotion Hashes
        private static int inputXHash = Animator.StringToHash("inputX");
        private static int inputYHash = Animator.StringToHash("inputY");
        private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
        private static int isIdleHash = Animator.StringToHash("isIdle");
        private static int isGroundedHash = Animator.StringToHash("isGrounded");
        private static int isFallingHash = Animator.StringToHash("isFalling");
        private static int isJumpingHash = Animator.StringToHash("isJumping");
        private static int isCrouchingHash = Animator.StringToHash("isCrouching");

        //Action Hashes
        private static int isAttackingHash = Animator.StringToHash("isAttacking");
        private static int isAimingHash = Animator.StringToHash("isAiming");
        private static int isInteractingHash = Animator.StringToHash("isInteracting");
        private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
        private int[] actionHashes;
        
        //Camera Rotation Hashes
        private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
        private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");

        private Vector3 _currentBlendInput = Vector3.zero;

        //Blend Tree state values
        private float _crouchMaxBlendValue = 0.5f;
        private float _walkMaxBlendValue = 0.75f;
        private float _runMaxBlendValue = 1f;
        private float _sprintMaxBlendValue = 1.5f;
        #endregion

#region Initizialize
        private void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerActionsInput = GetComponent<PlayerActionsInput>();
            _playerState = GetComponent<PlayerState>();
            _playerController = GetComponent<PlayerController>();

            actionHashes = new int[]
            {
                isInteractingHash
            };
        }
#endregion

#region Update
        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isIdle = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idle;
            bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;
            bool isCrouching = _playerState.CurrentPlayerMovementState == PlayerMovementState.Crouching;
            bool isRunning = _playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
            bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
            bool isGrounded = _playerState.InGroundedState();
            bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));

            bool isRunBlendValue = isRunning || isJumping || isFalling;

            Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * _sprintMaxBlendValue :
                                  isCrouching ? _playerLocomotionInput.MovementInput * _crouchMaxBlendValue :  
                                  isRunBlendValue ? _playerLocomotionInput.MovementInput * _runMaxBlendValue : 
                                                    _playerLocomotionInput.MovementInput * _walkMaxBlendValue;


            _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            //movement
            _animator.SetBool(isGroundedHash, isGrounded);
            _animator.SetBool(isIdleHash, isIdle);
            _animator.SetBool(isJumpingHash, isJumping);
            _animator.SetBool(isFallingHash, isFalling);
            _animator.SetBool(isCrouchingHash, isCrouching);
            _animator.SetFloat(inputXHash, _currentBlendInput.x);
            _animator.SetFloat(inputYHash, _currentBlendInput.y);
            _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);

                        //actions
            _animator.SetBool(isAttackingHash, _playerActionsInput.AttackPressed);
            _animator.SetBool(isAimingHash, _playerActionsInput.AimPressed);
            _animator.SetBool(isInteractingHash, _playerActionsInput.InteractPressed);
            _animator.SetBool(isPlayingActionHash, isPlayingAction);
            
            //camera rotation
            _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);
            _animator.SetFloat(rotationMismatchHash, _playerController.rotationMismatch);
        }
#endregion
    }
}

