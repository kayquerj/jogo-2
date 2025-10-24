using System;
using UnityEngine;

namespace Harvesting
{
    [CreateAssetMenu(fileName = "Resource Node", menuName = "Harvesting/Generic Resource Node")]
    public class ResourceNodeConfig : ScriptableObject
    {
        [SerializeField]
        private string displayName = "Resource";

        [SerializeField, Tooltip("Time in seconds required to harvest the node once.")]
        private float harvestDuration = 3f;

        [SerializeField, Tooltip("Delay in seconds before the node respawns after harvesting.")]
        private float respawnDelay = 10f;

        [SerializeField]
        private ResourceYield[] yields = new ResourceYield[] { new ResourceYield("default", 1) };

        public string DisplayName => displayName;
        public float HarvestDuration => Mathf.Max(0.01f, harvestDuration);
        public float RespawnDelay => Mathf.Max(0f, respawnDelay);
        public ResourceYield[] Yields => yields;

        public void Configure(string name, float harvestSeconds, float respawnSeconds, ResourceYield[] resourceYields)
        {
            displayName = string.IsNullOrWhiteSpace(name) ? displayName : name;
            harvestDuration = Mathf.Max(0.01f, harvestSeconds);
            respawnDelay = Mathf.Max(0f, respawnSeconds);
            yields = resourceYields == null ? Array.Empty<ResourceYield>() : (ResourceYield[])resourceYields.Clone();
        }
    }

    [CreateAssetMenu(fileName = "Tree Resource Node", menuName = "Harvesting/Tree Resource Node")]
    public class TreeResourceNodeConfig : ResourceNodeConfig
    {
    }

    [CreateAssetMenu(fileName = "Rock Resource Node", menuName = "Harvesting/Rock Resource Node")]
    public class RockResourceNodeConfig : ResourceNodeConfig
    {
    }
}
