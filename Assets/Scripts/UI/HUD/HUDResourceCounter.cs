using Game.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
    /// <summary>
    /// Displays a single resource entry within the HUD and handles gain feedback.
    /// </summary>
    public class HUDResourceCounter : MonoBehaviour
    {
        [SerializeField]
        private ResourceType resourceType = ResourceType.Wood;

        [SerializeField]
        private Text valueLabel;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Animator pulseAnimator;

        [SerializeField]
        private string pulseTriggerName = "Pulse";

        [SerializeField]
        private string pulseAmountParameter = "Amount";

        public ResourceType ResourceType => resourceType;

        public void SetValue(int value)
        {
            if (valueLabel != null)
            {
                valueLabel.text = value.ToString();
            }
        }

        public void SetIcon(Sprite sprite)
        {
            if (icon == null)
            {
                return;
            }

            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        public void PlayGainFeedback(int delta)
        {
            if (pulseAnimator == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(pulseAmountParameter) && HasIntegerParameter(pulseAnimator, pulseAmountParameter))
            {
                pulseAnimator.SetInteger(pulseAmountParameter, delta);
            }

            if (!string.IsNullOrEmpty(pulseTriggerName))
            {
                pulseAnimator.SetTrigger(pulseTriggerName);
            }
        }

        private static bool HasIntegerParameter(Animator animator, string parameterName)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Int && parameter.name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
