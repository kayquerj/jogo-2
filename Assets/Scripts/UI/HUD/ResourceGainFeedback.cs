using System.Collections;
using Game.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
    /// <summary>
    /// Spawns a simple floating label each time resources are gained to reinforce player feedback.
    /// </summary>
    public class ResourceGainFeedback : MonoBehaviour
    {
        [SerializeField]
        private RectTransform container;

        [SerializeField]
        private Text floatingLabelTemplate;

        [SerializeField]
        private float travelDistance = 48f;

        [SerializeField]
        private float lifetime = 1.2f;

        [SerializeField]
        private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private void Awake()
        {
            if (floatingLabelTemplate != null)
            {
                floatingLabelTemplate.gameObject.SetActive(false);
            }
        }

        public void Play(ResourceType resourceType, int amount)
        {
            if (amount <= 0 || floatingLabelTemplate == null || container == null)
            {
                return;
            }

            var instance = Instantiate(floatingLabelTemplate, container);
            instance.gameObject.SetActive(true);
            instance.text = $"+{amount} {resourceType}";

            StartCoroutine(Animate(instance));
        }

        private IEnumerator Animate(Text label)
        {
            var rectTransform = label.rectTransform;
            var startPosition = rectTransform.anchoredPosition;
            var targetPosition = startPosition + Vector2.up * travelDistance;
            var baseColor = label.color;
            var elapsed = 0f;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / lifetime);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, progress);

                var alpha = alphaCurve.Evaluate(progress);
                var color = baseColor;
                color.a = alpha;
                label.color = color;

                yield return null;
            }

            Destroy(label.gameObject);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}
