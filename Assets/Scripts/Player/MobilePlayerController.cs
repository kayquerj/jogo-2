using System;
using Game.Saving;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Mobile
{
    [RequireComponent(typeof(CharacterController))]
    public class MobilePlayerController : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        [Header("Input")]
        [Tooltip("Input actions asset that contains the movement bindings.")]
        [SerializeField] private InputActionAsset actionsAsset;
        [SerializeField] private string moveActionPath = "Gameplay/Move";
        [SerializeField] private string sprintActionPath = "Gameplay/Sprint";

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float walkSpeed = 3f;
        [SerializeField, Min(0f)] private float sprintSpeed = 5.5f;
        [SerializeField, Min(0.1f)] private float acceleration = 12f;
        [SerializeField, Range(0f, 1f)] private float analogDeadZone = 0.1f;
        [SerializeField, Min(1f)] private float rotationSpeed = 540f;
        [SerializeField] private bool alignMovementToCamera = true;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float gravityMultiplier = 1f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private float animationDampTime = 0.1f;

        private CharacterController characterController;
        private InputAction moveAction;
        private InputAction sprintAction;
        private float currentSpeed;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (!cameraTransform && alignMovementToCamera)
            {
                var mainCamera = Camera.main;
                if (mainCamera)
                {
                    cameraTransform = mainCamera.transform;
                }
            }

            if (!animator)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void OnEnable()
        {
            ResolveActions();

            moveAction?.Enable();
            sprintAction?.Enable();

            GameSaveManager.Instance?.RegisterPlayerTransform(transform);
        }

        private void OnDisable()
        {
            GameSaveManager.Instance?.UnregisterPlayerTransform(transform);

            moveAction?.Disable();
            sprintAction?.Disable();
        }

        private void Update()
        {
            if (moveAction == null)
            {
                return;
            }

            var moveInput = moveAction.ReadValue<Vector2>();
            if (moveInput.magnitude < analogDeadZone)
            {
                moveInput = Vector2.zero;
            }

            bool isSprinting = sprintAction != null && sprintAction.IsPressed();
            float targetSpeed = (isSprinting ? sprintSpeed : walkSpeed) * moveInput.magnitude;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            if (alignMovementToCamera && cameraTransform)
            {
                Vector3 cameraForward = cameraTransform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();

                Vector3 cameraRight = cameraTransform.right;
                cameraRight.y = 0f;
                cameraRight.Normalize();

                inputDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
            }

            Vector3 desiredMove = inputDirection.normalized * currentSpeed;

            if (desiredMove.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredMove, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            ApplyGravity();

            Vector3 motion = desiredMove;
            motion.y = verticalVelocity;

            characterController.Move(motion * Time.deltaTime);

            if (animator)
            {
                float normalizedSpeed = sprintSpeed > 0.01f ? Mathf.Clamp01(currentSpeed / sprintSpeed) : currentSpeed;
                animator.SetFloat(SpeedHash, normalizedSpeed, animationDampTime, Time.deltaTime);
            }
        }

        public void TeleportTo(Vector3 position)
        {
            bool wasEnabled = characterController.enabled;
            if (wasEnabled)
            {
                characterController.enabled = false;
            }

            transform.position = position;
            verticalVelocity = 0f;
            currentSpeed = 0f;

            if (wasEnabled)
            {
                characterController.enabled = true;
            }
        }

        private void ResolveActions()
        {
            if (!actionsAsset)
            {
                Debug.LogWarning($"{nameof(MobilePlayerController)} on {name} has no InputActionAsset assigned. Movement will not function.");
                return;
            }

            moveAction = null;
            sprintAction = null;

            if (!string.IsNullOrEmpty(moveActionPath))
            {
                try
                {
                    moveAction = actionsAsset.FindAction(moveActionPath, true);
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogError($"Move action '{moveActionPath}' could not be found in {actionsAsset.name}. {ex.Message}", this);
                }
            }

            if (!string.IsNullOrEmpty(sprintActionPath))
            {
                try
                {
                    sprintAction = actionsAsset.FindAction(sprintActionPath, false);
                    if (sprintAction == null)
                    {
                        Debug.LogWarning($"Sprint action '{sprintActionPath}' was not found in {actionsAsset.name}. Sprinting will be disabled.", this);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogWarning($"Sprint action '{sprintActionPath}' could not be resolved. {ex.Message}", this);
                }
            }
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded)
            {
                verticalVelocity = -1f;
                return;
            }

            float gravity = Physics.gravity.y * gravityMultiplier;
            verticalVelocity += gravity * Time.deltaTime;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            walkSpeed = Mathf.Max(0.1f, walkSpeed);
            sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
            acceleration = Mathf.Max(0.01f, acceleration);
            rotationSpeed = Mathf.Max(0f, rotationSpeed);
            analogDeadZone = Mathf.Clamp01(analogDeadZone);
            gravityMultiplier = Mathf.Max(0f, gravityMultiplier);

            if (!cameraTransform && alignMovementToCamera)
            {
                var mainCamera = Camera.main;
                if (mainCamera)
                {
                    cameraTransform = mainCamera.transform;
                }
            }
        }
#endif
    }
}
