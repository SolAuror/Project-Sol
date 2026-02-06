using UnityEngine;
using UnityEngine.InputSystem;

namespace Sol.FinalPlayerController
{
    public class PlayerActionsInput : MonoBehaviour, PlayerControls.IPlayerActionMapActions
    {
#region Class Variables
        [SerializeField] private bool IsAiming = false;
        public bool AttackPressed { get; private set; }
        public bool AimPressed { get; private set; }
        public bool InteractPressed { get; private set; }

        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerState _playerState;
        #endregion

#region Initizialize
        private void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
        }

        private void OnEnable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player Controls is not Initialized cannot Enable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Enable();
            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.AddCallbacks(this);
        }

        private void OnDisable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player Controls is not Initialized cannot Disable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Disable();
            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.RemoveCallbacks(this);
        }
#endregion

#region update
        private void Update()
        {
            if (_playerLocomotionInput.MovementInput != Vector2.zero ||
                _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping ||
                _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling ||
                _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting)
            {
                InteractPressed = false;
            }
        }
#endregion

#region late update
        public void SetInteractPressedFalse()
        {
            InteractPressed = false;
        }

        public void SetAttackPressedFalse()
        {
            AttackPressed = false;
        }
#endregion

#region InputCallbacks
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                AttackPressed = true;
                Debug.Log("Hyuah!");
            }
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.performed) //if hold to aim is true, or aim is off while pressed, then aim
            {
                AimPressed = IsAiming || !AimPressed;
                Debug.Log("Aiming!");
            }

            else if (context.canceled)  //if hold to aim is true and aim is released, then stop
            {
                AimPressed = !IsAiming && AimPressed;
                Debug.Log("Stopped Aiming");
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                InteractPressed = true;
                Debug.Log("Oooooh Shiny!");
            }
        }

        // Perspective swap is handled by PlayerInputManager
        public void OnPerspective(InputAction.CallbackContext context) { }
        #endregion
    }
}