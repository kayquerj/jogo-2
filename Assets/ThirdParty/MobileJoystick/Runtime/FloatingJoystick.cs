// This file is distributed under the MIT License. See LICENSE.md for details.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace ThirdParty.MobileJoystick
{
    [AddComponentMenu("Mobile/Input/Floating Joystick (Input System)")]
    public class FloatingJoystick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const string DefaultControlPath = "<Gamepad>/leftStick";

        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 80f;
        [SerializeField] private float deadZone = 0.1f;
        [SerializeField] private bool snapToFinger = true;
        [SerializeField] private Canvas targetCanvas;

        [SerializeField, InputControl(layout = "Vector2")]
        private string controlPath = DefaultControlPath;

        private Vector2 inputVector;
        private Camera uiCamera;

        public Vector2 Direction => inputVector;

        protected override string controlPathInternal
        {
            get => controlPath;
            set => controlPath = value;
        }

        private void Awake()
        {
            if (!background || !handle)
            {
                Debug.LogWarning("FloatingJoystick is missing required references.");
            }

            if (!targetCanvas)
            {
                targetCanvas = GetComponentInParent<Canvas>();
            }

            if (targetCanvas && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = targetCanvas.worldCamera;
            }

            SetVisible(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            RepositionBackground(eventData.position);
            UpdateHandle(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateHandle(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            SendValueToControl(Vector2.zero);
            SetVisible(false);
            if (handle)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }

        private void RepositionBackground(Vector2 screenPosition)
        {
            if (!background)
            {
                return;
            }

            if (snapToFinger)
            {
                Vector2 anchoredPosition;
                RectTransform parentRect = background.parent as RectTransform;
                if (parentRect)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, uiCamera, out anchoredPosition);
                }
                else
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(background, screenPosition, uiCamera, out anchoredPosition);
                }

                background.anchoredPosition = anchoredPosition;
            }

            SetVisible(true);
        }

        private void UpdateHandle(PointerEventData eventData)
        {
            if (!background || !handle)
            {
                return;
            }

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, uiCamera, out localPoint);
            Vector2 radius = background.sizeDelta * 0.5f;
            radius.x = Mathf.Approximately(radius.x, 0f) ? 1f : radius.x;
            radius.y = Mathf.Approximately(radius.y, 0f) ? 1f : radius.y;

            Vector2 normalised = new Vector2(localPoint.x / radius.x, localPoint.y / radius.y);
            inputVector = Vector2.ClampMagnitude(normalised, 1f);

            if (inputVector.magnitude < deadZone)
            {
                inputVector = Vector2.zero;
            }

            handle.anchoredPosition = inputVector * handleRange;
            SendValueToControl(inputVector);
        }

        private void OnDisable()
        {
            SendValueToControl(Vector2.zero);
            inputVector = Vector2.zero;
            if (handle)
            {
                handle.anchoredPosition = Vector2.zero;
            }

            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (!background)
            {
                return;
            }

            background.gameObject.SetActive(visible || !snapToFinger);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            handleRange = Mathf.Max(1f, handleRange);
            deadZone = Mathf.Clamp01(deadZone);
        }
#endif
    }
}
