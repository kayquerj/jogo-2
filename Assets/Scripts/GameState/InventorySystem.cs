using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Saving
{
    public class InventorySystem : MonoBehaviour
    {
        [Serializable]
        public struct InventoryItem
        {
            public string itemId;
            public int quantity;
        }

        [SerializeField]
        private List<InventoryItem> startingItems = new List<InventoryItem>();

        private readonly Dictionary<string, int> items = new Dictionary<string, int>();

        public event Action InventoryChanged;

        private void Awake()
        {
            RebuildFrom(startingItems);
        }

        private void OnEnable()
        {
            GameSaveManager.Instance?.RegisterInventorySystem(this);
        }

        private void OnDisable()
        {
            GameSaveManager.Instance?.UnregisterInventorySystem(this);
        }

        public IReadOnlyDictionary<string, int> Items => items;

        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return 0;
            }

            return items.TryGetValue(itemId, out var value) ? value : 0;
        }

        public void SetItemCount(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            amount = Mathf.Max(0, amount);
            if (items.TryGetValue(itemId, out var existing) && existing == amount)
            {
                return;
            }

            items[itemId] = amount;
            InventoryChanged?.Invoke();
        }

        public void AddItem(string itemId, int delta)
        {
            if (delta == 0)
            {
                return;
            }

            int newAmount = Mathf.Max(0, GetItemCount(itemId) + delta);
            SetItemCount(itemId, newAmount);
        }

        public List<InventoryItemData> CaptureState()
        {
            var snapshot = new List<InventoryItemData>(items.Count);
            foreach (var pair in items)
            {
                snapshot.Add(new InventoryItemData(pair.Key, pair.Value));
            }

            return snapshot;
        }

        public void RestoreState(IReadOnlyList<InventoryItemData> snapshot)
        {
            RestoreState(snapshot, false);
        }

        public void RestoreState(IReadOnlyList<InventoryItemData> snapshot, bool suppressEvents)
        {
            items.Clear();

            if (snapshot != null)
            {
                foreach (var entry in snapshot)
                {
                    if (string.IsNullOrEmpty(entry.itemId))
                    {
                        continue;
                    }

                    items[entry.itemId] = Mathf.Max(0, entry.quantity);
                }
            }

            if (!suppressEvents)
            {
                InventoryChanged?.Invoke();
            }
        }

        private void RebuildFrom(IEnumerable<InventoryItem> source)
        {
            items.Clear();
            foreach (var entry in source)
            {
                if (string.IsNullOrEmpty(entry.itemId))
                {
                    continue;
                }

                items[entry.itemId] = Mathf.Max(0, entry.quantity);
            }
        }
    }
}
