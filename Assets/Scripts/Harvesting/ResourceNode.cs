using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Harvesting
{
    public enum ResourceNodeState
    {
        Available,
        Harvesting,
        CoolingDown
    }

    public interface IHarvestAgent
    {
        void ReceiveResources(ResourceYield[] yields);
    }

    [RequireComponent(typeof(Collider))]
    public class ResourceNode : MonoBehaviour
    {
        [System.Serializable]
        public class HarvestProgressEvent : UnityEvent<float>
        {
        }

        [System.Serializable]
        public class ResourceNodeStateEvent : UnityEvent<ResourceNodeState>
        {
        }

        [SerializeField]
        private ResourceNodeConfig config;

        [SerializeField]
        private bool autoStartHarvest = true;

        [SerializeField]
        private bool cancelHarvestWhenAgentLeaves = true;

        [SerializeField]
        private bool awardResourcesDirectly = true;

        [SerializeField]
        private GameObject resourcePickupPrefab;

        [SerializeField]
        private Transform pickupSpawnPoint;

        [SerializeField]
        private HarvestProgressEvent onHarvestProgress = new HarvestProgressEvent();

        [SerializeField]
        private ResourceNodeStateEvent onStateChanged = new ResourceNodeStateEvent();

        private ResourceNodeState state = ResourceNodeState.Available;
        private Coroutine harvestCoroutine;
        private Coroutine respawnCoroutine;
        private IHarvestAgent activeAgent;
        private IHarvestAgent occupantAgent;
        private Collider proximityCollider;
        private float currentProgress;

        public ResourceNodeConfig Config
        {
            get => config;
            set => config = value;
        }

        public bool AutoStartHarvest
        {
            get => autoStartHarvest;
            set => autoStartHarvest = value;
        }

        public bool CancelHarvestWhenAgentLeaves
        {
            get => cancelHarvestWhenAgentLeaves;
            set => cancelHarvestWhenAgentLeaves = value;
        }

        public bool AwardResourcesDirectly
        {
            get => awardResourcesDirectly;
            set => awardResourcesDirectly = value;
        }

        public ResourceNodeState State => state;
        public float HarvestProgress => currentProgress;
        public HarvestProgressEvent OnHarvestProgress => onHarvestProgress;
        public ResourceNodeStateEvent OnStateChanged => onStateChanged;

        private void Awake()
        {
            proximityCollider = GetComponent<Collider>();
            if (proximityCollider != null)
            {
                proximityCollider.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            ResetNode();
        }

        private void OnDisable()
        {
            CancelHarvest(true);
            occupantAgent = null;
            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
                respawnCoroutine = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var agent = FindHarvestAgent(other);
            if (!IsAgentValid(agent))
            {
                return;
            }

            occupantAgent = agent;

            if (autoStartHarvest)
            {
                TryBeginHarvest(agent);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var agent = FindHarvestAgent(other);
            if (occupantAgent != null && agent == occupantAgent)
            {
                occupantAgent = null;
            }

            if (!cancelHarvestWhenAgentLeaves || activeAgent == null)
            {
                return;
            }

            if (!IsAgentValid(agent) || agent != activeAgent)
            {
                return;
            }

            CancelHarvest();
        }

        public bool TryBeginHarvest(IHarvestAgent agent)
        {
            if (config == null || !IsAgentValid(agent))
            {
                return false;
            }

            if (state != ResourceNodeState.Available)
            {
                return false;
            }

            if (occupantAgent == null)
            {
                occupantAgent = agent;
            }

            activeAgent = agent;
            if (harvestCoroutine != null)
            {
                StopCoroutine(harvestCoroutine);
            }

            harvestCoroutine = StartCoroutine(HarvestRoutine());
            return true;
        }

        public void CancelHarvest(bool dueToDisable = false)
        {
            if (state != ResourceNodeState.Harvesting)
            {
                return;
            }

            if (harvestCoroutine != null)
            {
                StopCoroutine(harvestCoroutine);
                harvestCoroutine = null;
            }

            activeAgent = null;
            SetState(ResourceNodeState.Available);
            currentProgress = 0f;
            onHarvestProgress.Invoke(currentProgress);

            if (!dueToDisable && respawnCoroutine == null && config != null && config.RespawnDelay > 0f)
            {
                // When cancelled we do not trigger respawn coroutine; node becomes immediately available
            }
        }

        private IEnumerator HarvestRoutine()
        {
            SetState(ResourceNodeState.Harvesting);
            currentProgress = 0f;
            onHarvestProgress.Invoke(currentProgress);

            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, config.HarvestDuration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                currentProgress = Mathf.Clamp01(elapsed / duration);
                onHarvestProgress.Invoke(currentProgress);
                yield return null;
            }

            currentProgress = 1f;
            onHarvestProgress.Invoke(currentProgress);
            harvestCoroutine = null;

            CompleteHarvest();
        }

        private void CompleteHarvest()
        {
            AwardResources();

            SetState(ResourceNodeState.CoolingDown);

            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
            }

            respawnCoroutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            float delay = Mathf.Max(0f, config.RespawnDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            respawnCoroutine = null;
            ResetNode();
        }

        private void ResetNode()
        {
            activeAgent = null;
            SetState(ResourceNodeState.Available);
            currentProgress = 0f;
            onHarvestProgress.Invoke(currentProgress);

            if (!IsAgentValid(occupantAgent))
            {
                occupantAgent = null;
                return;
            }

            if (autoStartHarvest)
            {
                TryBeginHarvest(occupantAgent);
            }
        }

        private void SetState(ResourceNodeState newState)
        {
            if (state == newState)
            {
                return;
            }

            state = newState;
            onStateChanged.Invoke(state);
        }

        private void AwardResources()
        {
            var yields = config != null ? config.Yields : null;
            if (yields == null || yields.Length == 0)
            {
                return;
            }

            if (!awardResourcesDirectly && resourcePickupPrefab != null)
            {
                Vector3 spawnPosition = pickupSpawnPoint != null ? pickupSpawnPoint.position : transform.position;
                Quaternion spawnRotation = pickupSpawnPoint != null ? pickupSpawnPoint.rotation : Quaternion.identity;

                var pickupInstance = Instantiate(resourcePickupPrefab, spawnPosition, spawnRotation);
                var pickup = pickupInstance.GetComponent<ResourcePickup>();
                if (pickup != null)
                {
                    pickup.SetYields(yields);
                }
            }
            else if (activeAgent != null)
            {
                activeAgent.ReceiveResources(yields);
            }
        }

        private static IHarvestAgent FindHarvestAgent(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            return collider.GetComponentInParent<IHarvestAgent>();
        }

        private static bool IsAgentValid(IHarvestAgent agent)
        {
            if (agent == null)
            {
                return false;
            }

            if (agent is Object unityObject)
            {
                return unityObject != null;
            }

            return true;
        }
    }
}
