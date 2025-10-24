using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Inventory;
using UnityEngine;

namespace Game.UI.HUD
{
    /// <summary>
    /// Glue layer coordinating inventory counters, harvest progress and prompts.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Inventory HUD")]
        [SerializeField]
        private HUDResourceCounter[] resourceCounters;

        [SerializeField]
        private ResourceGainFeedback gainFeedback;

        [Header("Interaction")]
        [SerializeField]
        private HarvestProgressDisplay harvestProgressDisplay;

        [SerializeField]
        private InteractionPromptView interactionPrompt;

        [SerializeField]
        private string defaultPrompt = "Hold to Harvest";

        private readonly Dictionary<ResourceType, HUDResourceCounter> counterLookup = new Dictionary<ResourceType, HUDResourceCounter>();
        private ResourceNode observedNode;

        private void Awake()
        {
            BuildLookup();
        }

        private void OnEnable()
        {
            var inventory = InventoryService.Instance;
            inventory.ResourceChanged += HandleResourceChanged;
            inventory.InventoryLoaded += HandleInventoryLoaded;

            if (inventory.IsInitialized)
            {
                SyncCountersWithInventory();
            }
        }

        private void Start()
        {
            if (!InventoryService.Instance.IsInitialized)
            {
                InventoryService.Instance.Load();
            }

            SyncCountersWithInventory();
        }

        private void OnDisable()
        {
            var inventory = InventoryService.Instance;
            inventory.ResourceChanged -= HandleResourceChanged;
            inventory.InventoryLoaded -= HandleInventoryLoaded;

            ObserveNode(null);
        }

        public void ObserveNode(ResourceNode node)
        {
            if (observedNode == node)
            {
                return;
            }

            if (observedNode != null)
            {
                observedNode.HarvestStarted -= HandleHarvestStarted;
                observedNode.HarvestCompleted -= HandleHarvestCompleted;
                observedNode.HarvestCancelled -= HandleHarvestCancelled;
                observedNode.NodeRespawned -= HandleNodeRespawned;
                observedNode.InteractionStateChanged -= HandleInteractionStateChanged;
            }

            harvestProgressDisplay?.Detach();

            observedNode = node;

            if (observedNode == null)
            {
                interactionPrompt?.Hide();
                return;
            }

            observedNode.HarvestStarted += HandleHarvestStarted;
            observedNode.HarvestCompleted += HandleHarvestCompleted;
            observedNode.HarvestCancelled += HandleHarvestCancelled;
            observedNode.NodeRespawned += HandleNodeRespawned;
            observedNode.InteractionStateChanged += HandleInteractionStateChanged;

            interactionPrompt?.Show(GetPrompt(observedNode));
        }

        private void BuildLookup()
        {
            counterLookup.Clear();

            if (resourceCounters == null)
            {
                return;
            }

            foreach (var counter in resourceCounters)
            {
                if (counter == null)
                {
                    continue;
                }

                counterLookup[counter.ResourceType] = counter;
                InventoryService.Instance.RegisterResource(counter.ResourceType, InventoryService.Instance.GetQuantity(counter.ResourceType));
                counter.SetValue(InventoryService.Instance.GetQuantity(counter.ResourceType));
            }
        }

        private void SyncCountersWithInventory()
        {
            foreach (var pair in counterLookup)
            {
                pair.Value.SetValue(InventoryService.Instance.GetQuantity(pair.Key));
            }
        }

        private void HandleResourceChanged(ResourceChangedEvent change)
        {
            if (!counterLookup.TryGetValue(change.Type, out var counter))
            {
                return;
            }

            counter.SetValue(change.NewValue);

            if (change.IsGain)
            {
                counter.PlayGainFeedback(change.Delta);
                gainFeedback?.Play(change.Type, change.Delta);
            }
        }

        private void HandleInventoryLoaded(InventorySnapshot snapshot)
        {
            SyncCountersWithInventory();
        }

        private void HandleHarvestStarted(ResourceNode node)
        {
            harvestProgressDisplay?.Track(node);
            interactionPrompt?.Hide();
        }

        private void HandleHarvestCompleted(ResourceNode node)
        {
            harvestProgressDisplay?.Detach();

            if (!node.IsDepleted)
            {
                interactionPrompt?.Show(GetPrompt(node));
            }
            else
            {
                interactionPrompt?.Hide();
            }
        }

        private void HandleHarvestCancelled(ResourceNode node)
        {
            harvestProgressDisplay?.Detach();
            interactionPrompt?.Show(GetPrompt(node));
        }

        private void HandleNodeRespawned(ResourceNode node)
        {
            interactionPrompt?.Show(GetPrompt(node));
        }

        private string GetPrompt(ResourceNode node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.InteractionPrompt))
            {
                return defaultPrompt;
            }

            return node.InteractionPrompt;
        }

        private void HandleInteractionStateChanged(ResourceNode node, bool canInteract)
        {
            if (canInteract)
            {
                interactionPrompt?.Show(GetPrompt(node));
            }
            else
            {
                interactionPrompt?.Hide();
            }
        }
    }
}
