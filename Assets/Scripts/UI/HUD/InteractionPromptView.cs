using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
    /// <summary>
    /// Handles the presentation of interaction prompts (e.g. "Hold to Harvest").
    /// </summary>
    public class InteractionPromptView : MonoBehaviour
    {
        [SerializeField]
        private Text promptLabel;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private float fadeDuration = 0.15f;

        private Coroutine fadeRoutine;

        public void Show(string prompt)
        {
            if (promptLabel != null)
            {
                promptLabel.text = prompt;
            }

            FadeTo(1f);
        }

        public void Hide()
        {
            FadeTo(0f);
        }

        private void FadeTo(float targetAlpha)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            var startAlpha = canvasGroup.alpha;
            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                var progress = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = targetAlpha > 0.9f;
            canvasGroup.blocksRaycasts = canvasGroup.interactable;
            fadeRoutine = null;
        }

        private void OnDisable()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
