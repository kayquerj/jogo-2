using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Game.Inventory;

namespace Game.Gameplay
{
    /// <summary>
    /// Represents a harvestable resource node. Handles timing, progress updates and
    /// deposits rewards into the inventory service when the harvest completes.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResourceNode : MonoBehaviour
    {
        private static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        [Header("Resource Settings")]
        [SerializeField]
        private ResourceType resourceType = ResourceType.Wood;

        [SerializeField]
        private int amountPerHarvest = 1;

        [SerializeField]
        private int rewardVariance;

        [Header("Harvest Settings")]
        [SerializeField]
        private float harvestDuration = 2.5f;

        [SerializeField]
        private AnimationCurve harvestEase = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        private int maxHarvests = -1;

        [SerializeField]
        private float respawnDelay = 5f;

        [SerializeField, TextArea(1, 2)]
        private string interactionPrompt = "Hold to Harvest";

        [Header("Feedback")]
        [SerializeField]
        private UnityEvent harvestStarted;

        [SerializeField]
        private UnityEvent<float> harvestProgressed;

        [SerializeField]
        private UnityEvent<ResourceType> harvestCompleted;

        [SerializeField]
        private UnityEvent nodeDepleted;

        [SerializeField]
        private UnityEvent nodeRespawned;

        public event System.Action<ResourceNode> HarvestStarted;
        public event System.Action<ResourceNode, float> HarvestProgress;
        public event System.Action<ResourceNode> HarvestCompleted;
        public event System.Action<ResourceNode> HarvestCancelled;
        public event System.Action<ResourceNode> NodeRespawned;
        public event System.Action<ResourceNode, bool> InteractionStateChanged;

        public ResourceType ResourceType => resourceType;

        public string InteractionPrompt => interactionPrompt;

        public int AmountPerHarvest => amountPerHarvest;

        public float HarvestDuration => harvestDuration;

        public bool IsHarvesting => harvestRoutine != null;

        public bool IsDepleted => maxHarvests >= 0 && harvestsCompleted >= maxHarvests;

        private Coroutine harvestRoutine;
        private Coroutine respawnRoutine;
        private int harvestsCompleted;
        private bool interactionState;

        public bool TryStartHarvest()
        {
            if (IsHarvesting || IsDepleted)
            {
                return false;
            }

            harvestRoutine = StartCoroutine(HarvestRoutine());
            harvestStarted?.Invoke();
            HarvestStarted?.Invoke(this);
            return true;
        }

        public void CancelHarvest()
        {
            if (harvestRoutine == null)
            {
                return;
            }

            StopCoroutine(harvestRoutine);
            harvestRoutine = null;
            harvestProgressed?.Invoke(0f);
            HarvestProgress?.Invoke(this, 0f);
            HarvestCancelled?.Invoke(this);
        }

        public void SetInteractionState(bool canInteract)
        {
            if (interactionState == canInteract)
            {
                return;
            }

            interactionState = canInteract;
            InteractionStateChanged?.Invoke(this, interactionState);
        }

        private IEnumerator HarvestRoutine()
        {
            var elapsed = 0f;

            while (elapsed < harvestDuration)
            {
                elapsed += Time.deltaTime;
                var rawProgress = harvestDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / harvestDuration);
                var easedProgress = harvestEase.Evaluate(rawProgress);

                harvestProgressed?.Invoke(easedProgress);
                HarvestProgress?.Invoke(this, easedProgress);
                yield return WaitForEndOfFrame;
            }

            GrantReward();
            harvestRoutine = null;
        }

        private void GrantReward()
        {
            var reward = amountPerHarvest;
            if (rewardVariance > 0)
            {
                reward += Random.Range(-rewardVariance, rewardVariance + 1);
                reward = Mathf.Max(0, reward);
            }

            if (reward > 0)
            {
                InventoryService.Instance.AddResource(resourceType, reward);
            }

            harvestsCompleted++;
            harvestCompleted?.Invoke(resourceType);
            HarvestCompleted?.Invoke(this);

            if (IsDepleted)
            {
                nodeDepleted?.Invoke();
                return;
            }

            if (respawnDelay > 0f)
            {
                if (respawnRoutine != null)
                {
                    StopCoroutine(respawnRoutine);
                }

                respawnRoutine = StartCoroutine(RespawnRoutine());
            }
            else
            {
                nodeRespawned?.Invoke();
                NodeRespawned?.Invoke(this);
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            respawnRoutine = null;
            nodeRespawned?.Invoke();
            NodeRespawned?.Invoke(this);
        }

        private void OnDisable()
        {
            if (harvestRoutine != null)
            {
                StopCoroutine(harvestRoutine);
                harvestRoutine = null;
                harvestProgressed?.Invoke(0f);
                HarvestProgress?.Invoke(this, 0f);
                HarvestCancelled?.Invoke(this);
            }

            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
                respawnRoutine = null;
            }
        }
    }
}
