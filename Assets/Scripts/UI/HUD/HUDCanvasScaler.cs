using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
    /// <summary>
    /// Configures a CanvasScaler with sensible defaults for both portrait and landscape mobile screens.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class HUDCanvasScaler : MonoBehaviour
    {
        [SerializeField]
        private Vector2 portraitReferenceResolution = new Vector2(1080f, 1920f);

        [SerializeField]
        private Vector2 landscapeReferenceResolution = new Vector2(1920f, 1080f);

        [SerializeField, Range(0f, 1f)]
        private float portraitMatch = 0.4f;

        [SerializeField, Range(0f, 1f)]
        private float landscapeMatch = 0.2f;

        [SerializeField]
        private bool adaptInRealtime = true;

        [SerializeField]
        private float landscapeThreshold = 1.1f;

        private CanvasScaler canvasScaler;
        private int lastWidth;
        private int lastHeight;

        private void Awake()
        {
            canvasScaler = GetComponent<CanvasScaler>();
            ApplyScale(Screen.width, Screen.height);
        }

        private void Update()
        {
            if (!adaptInRealtime)
            {
                return;
            }

            if (Screen.width == lastWidth && Screen.height == lastHeight)
            {
                return;
            }

            ApplyScale(Screen.width, Screen.height);
        }

        private void ApplyScale(int width, int height)
        {
            if (canvasScaler == null)
            {
                return;
            }

            lastWidth = width;
            lastHeight = height;

            var aspect = height == 0 ? 0f : (float)width / height;
            var isLandscape = aspect >= landscapeThreshold;

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.referenceResolution = isLandscape ? landscapeReferenceResolution : portraitReferenceResolution;
            canvasScaler.matchWidthOrHeight = isLandscape ? landscapeMatch : portraitMatch;
        }
    }
}
