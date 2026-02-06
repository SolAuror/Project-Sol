using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sol.FinalPlayerController
{
    [DefaultExecutionOrder(-3)]
    public class PlayerInputManager : MonoBehaviour, PlayerControls.IPlayerActionMapActions
    {
        public static PlayerInputManager Instance;
        public PlayerControls PlayerControls { get; private set; }

        [Header("Camera Setup")]
        [SerializeField] private CinemachineBrain _cinemachineBrain;
        [SerializeField] private CinemachineCamera _thirdPersonCamera;
        [SerializeField] private CinemachineCamera _firstPersonCamera;
        [SerializeField] private float _perspectiveSwapDelay = -1f;

        [Header("Player Reference")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Renderer _headMeshRenderer;

        private bool _isFirstPerson = false;
        private bool _isTransitioning = false;

        public bool IsFirstPerson => _isFirstPerson;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            
            if (_playerController == null)
                Debug.LogWarning("PlayerInputManager: PlayerController reference is missing. Camera switching will not work.");
            
            SetPerspective(_isFirstPerson);
        }

        private void OnEnable()
        {
            PlayerControls = new PlayerControls();
            PlayerControls.Enable();
            PlayerControls.PlayerActionMap.AddCallbacks(this);
        }

        private void OnDisable()
        {
            PlayerControls.PlayerActionMap.RemoveCallbacks(this);
            PlayerControls.Disable();
        }

        #region Perspective Swap
        private void SwapPerspective()
        {
            _isFirstPerson = !_isFirstPerson;
            SetPerspective(_isFirstPerson);
            Debug.Log($"Swapped to {(_isFirstPerson ? "First Person" : "Third Person")}");
        }

        private void SetPerspective(bool firstPerson)
        {
            CinemachineCamera activeCamera = firstPerson ? _firstPersonCamera : _thirdPersonCamera;
            CinemachineCamera inactiveCamera = firstPerson ? _thirdPersonCamera : _firstPersonCamera;

            if (inactiveCamera != null)
                inactiveCamera.Priority = 0;

            if (activeCamera != null)
                activeCamera.Priority = 10;

            _isTransitioning = true;

            // Update PlayerController camera reference and sync rotation
            if (_playerController != null && activeCamera != null)
            {
                // Capture current look direction from old camera
                Vector2 currentRotation = _playerController.CameraRotation;
                
                // Update to new camera transform
                _playerController.CamTransform = activeCamera.transform;
                
                // Restore rotation to maintain look direction
                _playerController.CameraRotation = currentRotation;
            }

            // Update head mesh shadow casting post-transition
            if (_headMeshRenderer != null)
            {
                if (!firstPerson)
                {
                    // Show head immediately when switching to third person
                    _headMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
            }

            // Always wait for the blend to finish and clear transitioning flag, even when head renderer is missing
            StartCoroutine(HandleTransitionEnd(firstPerson));
        }

        private System.Collections.IEnumerator HandleTransitionEnd(bool firstPerson)
        {
            //override delay when provided, Else wait for cinemachine blend to finish
            float waitTime;
            if (_perspectiveSwapDelay >= 0f)
                waitTime = _perspectiveSwapDelay;
            else
                waitTime = _cinemachineBrain != null ? _cinemachineBrain.DefaultBlend.Time : 0.5f;
            yield return new WaitForSeconds(waitTime);

            if (_headMeshRenderer != null)
            {
                if (firstPerson)
                    _headMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                else
                    _headMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            // Allow swaps again
            _isTransitioning = false;
        }
        #endregion

        #region Input Callbacks
        public void OnPerspective(InputAction.CallbackContext context)
        {
            if (context.performed && !_isTransitioning)
            {
                SwapPerspective();
            }
        }

        public void OnAttack(InputAction.CallbackContext context) { }
        public void OnAim(InputAction.CallbackContext context) { }
        public void OnInteract(InputAction.CallbackContext context) { }
        #endregion
    }
}