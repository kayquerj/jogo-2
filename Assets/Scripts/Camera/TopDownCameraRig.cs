using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Mobile
{
    [DisallowMultipleComponent]
    public class TopDownCameraRig : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 8f, -6f);
        [SerializeField] private float followLerpSpeed = 6f;

        [Header("Orbit")]
        [SerializeField] private float orbitSpeed = 90f;
        [SerializeField] private float tiltSpeed = 60f;
        [SerializeField] private float minTilt = 25f;
        [SerializeField] private float maxTilt = 70f;
        [SerializeField] private float minDistance = 4f;
        [SerializeField] private float maxDistance = 12f;
        [SerializeField] private float zoomLerpSpeed = 6f;

        [Header("Input")]
        [SerializeField] private InputActionAsset actionsAsset;
        [SerializeField] private string lookActionPath = "Gameplay/Look";
        [SerializeField] private string zoomActionPath = "Gameplay/Zoom";

        private InputAction lookAction;
        private InputAction zoomAction;
        private float currentYaw;
        private float currentTilt;
        private float currentDistance;

        private void Awake()
        {
            if (followOffset.sqrMagnitude < 0.001f)
            {
                followOffset = new Vector3(0f, 8f, -6f);
            }

            Vector3 initial = followOffset;
            currentDistance = initial.magnitude;
            currentTilt = Vector3.Angle(Vector3.ProjectOnPlane(-initial, Vector3.up), -initial);
            if (float.IsNaN(currentTilt))
            {
                currentTilt = 45f;
            }

            currentYaw = transform.eulerAngles.y;
        }

        private void OnEnable()
        {
            ResolveActions();
            lookAction?.Enable();
            zoomAction?.Enable();
        }

        private void OnDisable()
        {
            lookAction?.Disable();
            zoomAction?.Disable();
        }

        private void LateUpdate()
        {
            if (!followTarget)
            {
                return;
            }

            if (lookAction != null)
            {
                Vector2 look = lookAction.ReadValue<Vector2>();
                currentYaw += look.x * orbitSpeed * Time.deltaTime;
                currentTilt = Mathf.Clamp(currentTilt - look.y * tiltSpeed * Time.deltaTime, minTilt, maxTilt);
            }

            if (zoomAction != null)
            {
                float zoomValue = zoomAction.ReadValue<float>();
                currentDistance = Mathf.Clamp(currentDistance - zoomValue * zoomLerpSpeed * Time.deltaTime, minDistance, maxDistance);
            }

            Quaternion orbitRotation = Quaternion.Euler(currentTilt, currentYaw, 0f);
            Vector3 desiredPosition = followTarget.position - orbitRotation * Vector3.forward * currentDistance;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followLerpSpeed * Time.deltaTime));
            transform.rotation = orbitRotation;
        }

        public void SetTarget(Transform newTarget)
        {
            followTarget = newTarget;
        }

        private void ResolveActions()
        {
            if (!actionsAsset)
            {
                return;
            }

            lookAction = !string.IsNullOrEmpty(lookActionPath) ? actionsAsset.FindAction(lookActionPath, false) : null;
            zoomAction = !string.IsNullOrEmpty(zoomActionPath) ? actionsAsset.FindAction(zoomActionPath, false) : null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            minTilt = Mathf.Clamp(minTilt, 1f, 89f);
            maxTilt = Mathf.Clamp(maxTilt, minTilt + 1f, 89f);
            minDistance = Mathf.Max(0.1f, minDistance);
            maxDistance = Mathf.Max(minDistance + 0.1f, maxDistance);

            if (followOffset.sqrMagnitude < 0.001f)
            {
                followOffset = new Vector3(0f, Mathf.Max(1f, maxDistance * 0.7f), -Mathf.Max(2f, maxDistance * 0.5f));
            }
        }
#endif
    }
}
