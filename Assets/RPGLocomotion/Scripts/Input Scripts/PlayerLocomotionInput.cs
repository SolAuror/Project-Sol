using UnityEngine;
using UnityEngine.InputSystem;

namespace Character.RPGLocomotion
{
    [DefaultExecutionOrder(-2)]
    public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
    {
#region Class Variables
        [SerializeField] private bool holdToSprint = true;
        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool CrouchToggle { get; private set; }
        public bool SprintPressed { get; private set; }
        public bool WalkToggle { get; private set; }
#endregion

#region Initizialize
        private void OnEnable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player Controls is not Initialized cannot Enable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.Enable();
            PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player Controls is not Initialized cannot Disable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.Disable();
            PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.RemoveCallbacks(this);
        }
#endregion

#region late update
        private void LateUpdate()
        {
            JumpPressed = false;
        }
#endregion

#region InputCallbacks
        public void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }
        
        public void OnMovement(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed) //if hold to sprint is true, or sprint is off while pressed, then sprint
            {
                SprintPressed = holdToSprint || !SprintPressed;
                Debug.Log("YOU ARE SPEEDING!");
            }

            else if (context.canceled)  //if hold to sprint is false and sprint is on, then stop
            {
                SprintPressed = !holdToSprint && SprintPressed;
            }
        }

        public void OnWalk(InputAction.CallbackContext context)
        {
            if (context.performed)  //if sprint and walk are off, then walk
            {
                WalkToggle = !WalkToggle;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            JumpPressed = true;
            // Clear crouch immediately when jumping so we don't remain crouched on jump/landing frames
            CrouchToggle = false;
            Debug.Log($"[Input] frame {Time.frameCount} t={Time.time:F3} JumpPressed: cleared CrouchToggle");
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            CrouchToggle = !CrouchToggle;
            Debug.Log("Sneaky ;)");
        }
        #endregion
    }
}