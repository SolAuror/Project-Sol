using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sol.FinalPlayerController
{
    [DefaultExecutionOrder(-2)]
    public class ThirdPersonInput : MonoBehaviour, PlayerControls.IThirdPersonMapActions
    {
        #region Class Variables
        [SerializeField] private CinemachineCamera _cineCam;
        [SerializeField] private float _zoomSpeed = 2f;
        [SerializeField] private float _zoomLerpSpeed = 10f;
        [SerializeField] private float _cameraMinZoom = 1f;
        [SerializeField] private float _cameraMaxZoom = 5f;

        private CinemachineOrbitalFollow _orbital;
        private Vector2 _scrollDelta;
        private float _dpadDelta;

        private float _targetZoom;
        private float _currentZoom;
        #endregion

        #region Initizialize
        private void Awake()
        {
            
        } 

        private void Start()
        {
            _cineCam = GetComponent<CinemachineCamera>();
            _orbital = GetComponent<CinemachineOrbitalFollow>();

            if (_cineCam == null)
                Debug.LogError("ThirdPersonInput: CinemachineCamera component not found. This script must be on the same GameObject as the Cinemachine camera.");
            
            if (_orbital == null)
                Debug.LogError("ThirdPersonInput: CinemachineOrbitalFollow component not found. This script requires an orbital camera setup.");

            if (_orbital != null)
                _targetZoom = _currentZoom = _orbital.Radius;
        }

        private void OnEnable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player Controls is not Initialized cannot Enable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player Controls is not Initialized cannot Disable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Disable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.RemoveCallbacks(this);
        }
        #endregion

        #region update
        private void Update()
        {
            if (_scrollDelta.y != 0)
            {
                if (_orbital != null)
                {
                    _targetZoom = Mathf.Clamp(_orbital.Radius - _scrollDelta.y * _zoomSpeed, _cameraMinZoom, _cameraMaxZoom);
                    
                }
            }

            if (_dpadDelta != 0)
                
                if (_orbital != null)
                {
                _targetZoom = Mathf.Clamp(_orbital.Radius - _dpadDelta * _zoomSpeed, _cameraMinZoom, _cameraMaxZoom);
                
                }

            _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.deltaTime * _zoomLerpSpeed);
            _orbital.Radius = _currentZoom;
        }
        #endregion

        #region late update
        private void LateUpdate()
        {
            _scrollDelta = Vector2.zero;
            _dpadDelta = 0;
        }
        #endregion

        #region InputCallbacks
        void PlayerControls.IThirdPersonMapActions.OnMouseZoom(InputAction.CallbackContext context)
        {
            OnMouseZoom(context);
        }

        void PlayerControls.IThirdPersonMapActions.OnGamepadZoom(InputAction.CallbackContext context)
        {
            OnGamepadZoom(context);
        }

        private void OnMouseZoom(InputAction.CallbackContext context)
        {
            _scrollDelta = context.ReadValue<Vector2>();
        }


        public void OnGamepadZoom(InputAction.CallbackContext context)
        {
            _dpadDelta = context.ReadValue<float>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            
        }
        #endregion
    }
}