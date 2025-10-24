using System;
using System.Collections.Generic;

namespace Game.Inventory
{
    /// <summary>
    /// Centralised service that tracks resource totals, dispatches change events and
    /// coordinates persistence. Designed as a lightweight runtime singleton so that it
    /// can be accessed from gameplay objects and UI alike.
    /// </summary>
    public sealed class InventoryService
    {
        private static InventoryService instance;

        private readonly Dictionary<ResourceType, int> resourceLedger = new Dictionary<ResourceType, int>();

        private InventoryService()
        {
        }

        public static InventoryService Instance => instance ?? (instance = new InventoryService());

        public event Action<ResourceChangedEvent> ResourceChanged;

        public event Action<InventorySnapshot> InventoryLoaded;

        public event Action<InventorySnapshot> InventorySaved;

        public bool IsInitialized { get; private set; }

        public IInventoryPersistence Persistence { get; private set; }

        public void ConfigurePersistence(IInventoryPersistence persistence)
        {
            Persistence = persistence;
        }

        public void RegisterResource(ResourceType type, int initialQuantity = 0)
        {
            if (resourceLedger.ContainsKey(type))
            {
                SetQuantity(type, Math.Max(0, initialQuantity));
                return;
            }

            resourceLedger[type] = Math.Max(0, initialQuantity);
            ResourceChanged?.Invoke(new ResourceChangedEvent(type, 0, resourceLedger[type]));
        }

        public int GetQuantity(ResourceType type)
        {
            return resourceLedger.TryGetValue(type, out var value) ? value : 0;
        }

        public void SetQuantity(ResourceType type, int quantity)
        {
            quantity = Math.Max(0, quantity);
            var previousValue = GetQuantity(type);

            resourceLedger[type] = quantity;

            if (previousValue != quantity)
            {
                ResourceChanged?.Invoke(new ResourceChangedEvent(type, previousValue, quantity));
            }
        }

        public void AddResource(ResourceType type, int amount)
        {
            if (amount == 0)
            {
                return;
            }

            var current = GetQuantity(type);
            SetQuantity(type, current + amount);
        }

        public bool TrySpendResource(ResourceType type, int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            var current = GetQuantity(type);
            if (current < amount)
            {
                return false;
            }

            SetQuantity(type, current - amount);
            return true;
        }

        public InventorySnapshot CreateSnapshot()
        {
            return InventorySnapshot.FromDictionary(resourceLedger);
        }

        public void RestoreSnapshot(InventorySnapshot snapshot)
        {
            var previousState = new Dictionary<ResourceType, int>(resourceLedger);
            resourceLedger.Clear();

            if (snapshot != null)
            {
                var snapshotValues = snapshot.ToDictionary();
                foreach (var pair in snapshotValues)
                {
                    resourceLedger[pair.Key] = Math.Max(0, pair.Value);
                }
            }

            var notificationKeys = new HashSet<ResourceType>(previousState.Keys);
            foreach (var key in resourceLedger.Keys)
            {
                notificationKeys.Add(key);
            }

            foreach (var key in notificationKeys)
            {
                previousState.TryGetValue(key, out var previousValue);
                var newValue = GetQuantity(key);

                if (previousValue != newValue)
                {
                    ResourceChanged?.Invoke(new ResourceChangedEvent(key, previousValue, newValue));
                }
            }
        }

        public void Load()
        {
            InventorySnapshot snapshot = null;

            if (Persistence != null)
            {
                snapshot = Persistence.LoadSnapshot();
                if (snapshot != null)
                {
                    RestoreSnapshot(snapshot);
                }
            }

            var finalSnapshot = snapshot ?? CreateSnapshot();
            InventoryLoaded?.Invoke(finalSnapshot);
            IsInitialized = true;
        }

        public void Save()
        {
            if (Persistence == null)
            {
                return;
            }

            var snapshot = CreateSnapshot();
            Persistence.SaveSnapshot(snapshot);
            InventorySaved?.Invoke(snapshot);
        }

        public void Clear()
        {
            resourceLedger.Clear();
        }
    }
}
