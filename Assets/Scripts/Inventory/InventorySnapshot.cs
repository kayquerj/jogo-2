using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Inventory
{
    /// <summary>
    /// Serializable representation of all resource quantities tracked by the inventory service.
    /// </summary>
    [Serializable]
    public class InventorySnapshot
    {
        [SerializeField]
        private List<ResourceCount> resources = new List<ResourceCount>();

        public IReadOnlyList<ResourceCount> Resources => resources;

        public InventorySnapshot()
        {
        }

        public InventorySnapshot(IEnumerable<ResourceCount> counts)
        {
            if (counts == null)
            {
                resources = new List<ResourceCount>();
                return;
            }

            resources = new List<ResourceCount>(counts);
        }

        public int GetQuantity(ResourceType type)
        {
            for (var i = 0; i < resources.Count; i++)
            {
                if (resources[i].Type == type)
                {
                    return resources[i].Quantity;
                }
            }

            return 0;
        }

        public void SetQuantity(ResourceType type, int quantity)
        {
            quantity = Mathf.Max(0, quantity);
            for (var i = 0; i < resources.Count; i++)
            {
                if (resources[i].Type == type)
                {
                    resources[i] = new ResourceCount(type, quantity);
                    return;
                }
            }

            resources.Add(new ResourceCount(type, quantity));
        }

        public Dictionary<ResourceType, int> ToDictionary()
        {
            var dictionary = new Dictionary<ResourceType, int>();
            for (var i = 0; i < resources.Count; i++)
            {
                dictionary[resources[i].Type] = resources[i].Quantity;
            }

            return dictionary;
        }

        public static InventorySnapshot FromDictionary(IDictionary<ResourceType, int> source)
        {
            var snapshot = new InventorySnapshot();

            if (source == null)
            {
                return snapshot;
            }

            foreach (var pair in source)
            {
                snapshot.SetQuantity(pair.Key, pair.Value);
            }

            return snapshot;
        }
    }

    [Serializable]
    public readonly struct ResourceCount
    {
        public readonly ResourceType Type;
        public readonly int Quantity;

        public ResourceCount(ResourceType type, int quantity)
        {
            Type = type;
            Quantity = Mathf.Max(0, quantity);
        }
    }
}
