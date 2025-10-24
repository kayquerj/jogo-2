using System.Collections;
using Game.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
    /// <summary>
    /// Bridges a resource node to a UI slider that visualises harvest progress.
    /// </summary>
    public class HarvestProgressDisplay : MonoBehaviour
    {
        [SerializeField]
        private Slider progressSlider;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private float fadeDuration = 0.18f;

        private Coroutine fadeRoutine;
        private ResourceNode trackedNode;

        private void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (progressSlider != null)
            {
                progressSlider.value = 0f;
            }
        }

        public void Track(ResourceNode node)
        {
            if (trackedNode == node)
            {
                return;
            }

            Unsubscribe(trackedNode);
            trackedNode = node;

            if (trackedNode == null)
            {
                SetProgress(0f);
                Hide();
                return;
            }

            trackedNode.HarvestProgress += HandleHarvestProgress;
            trackedNode.HarvestCompleted += HandleHarvestCompleted;
            trackedNode.HarvestCancelled += HandleHarvestCancelled;

            SetProgress(0f);
            Show();
        }

        public void Detach()
        {
            Track(null);
        }

        private void HandleHarvestProgress(ResourceNode node, float progress)
        {
            SetProgress(progress);
        }

        private void HandleHarvestCompleted(ResourceNode node)
        {
            SetProgress(1f);
            Hide();
        }

        private void HandleHarvestCancelled(ResourceNode node)
        {
            Hide();
        }

        private void SetProgress(float progress)
        {
            if (progressSlider != null)
            {
                progressSlider.normalizedValue = Mathf.Clamp01(progress);
            }
        }

        private void Show()
        {
            FadeTo(1f);
        }

        private void Hide()
        {
            FadeTo(0f);
        }

        private void FadeTo(float targetAlpha)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (Mathf.Approximately(canvasGroup.alpha, targetAlpha))
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

        private void Unsubscribe(ResourceNode node)
        {
            if (node == null)
            {
                return;
            }

            node.HarvestProgress -= HandleHarvestProgress;
            node.HarvestCompleted -= HandleHarvestCompleted;
            node.HarvestCancelled -= HandleHarvestCancelled;
        }

        private void OnDisable()
        {
            FadeTo(0f);
        }

        private void OnDestroy()
        {
            Unsubscribe(trackedNode);
        }
    }
}
