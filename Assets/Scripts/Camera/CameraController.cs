using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float cameraMovementSpeed = 10f;
    
        [SerializeField] private float cameraMoveThreshold = 10f;

        private CinemachineBrain cinemachineBrain;
    
        private CinemachineVirtualCamera cinemachineVirtualCamera;

        private Vector3 startCameraPosition;
        private Quaternion startCameraRotation;

        private void Awake()
        {
            Application.runInBackground = true; 
            cinemachineBrain = GetComponent<CinemachineBrain>();
            cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            startCameraPosition = transform.localPosition;
            startCameraRotation = transform.localRotation;
        }

        public void SetTarget(Transform followTarget, Transform lookTarget)
        {
            cinemachineVirtualCamera.Follow = followTarget;
            cinemachineVirtualCamera.LookAt = lookTarget;
            
            if(followTarget == null && lookTarget == null)
            {
                transform.localPosition = startCameraPosition;
                transform.localRotation = startCameraRotation;
            }
        }

        public void Update()
        {
            if(cinemachineBrain.enabled) return;

            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

            Vector2 moveVector = Vector2.zero;
            if (mouseScreenPosition.x < cameraMoveThreshold)
            {
                moveVector.x = -1f;
            }
            else if (mouseScreenPosition.x > Screen.width - cameraMoveThreshold)
            {
                moveVector.x = 1f;
            }else if (mouseScreenPosition.y < cameraMoveThreshold)
            {
                moveVector.y = -1f;
            }
            else if (mouseScreenPosition.y > Screen.height - cameraMoveThreshold)
            {
                moveVector.y = 1f;
            }

            Vector3 cameraForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            transform.position += (transform.right * moveVector.x + cameraForward * moveVector.y) * cameraMovementSpeed * Time.deltaTime;
        }

        public void HandleLockInput(InputAction.CallbackContext value)
        {
        
#if UNITY_EDITOR
            if (value.started)
            {
                cinemachineBrain.enabled = !cinemachineBrain.enabled;
                cinemachineVirtualCamera.enabled = cinemachineBrain.enabled;
            }
#else
        if (value.started)
        {
            cinemachineBrain.enabled = true;
            cinemachineVirtualCamera.enabled = true;
        }

        if (value.canceled)
        {
            cinemachineBrain.enabled = false;
            cinemachineVirtualCamera.enabled = false;
        }
#endif
        
        }
    }
}
