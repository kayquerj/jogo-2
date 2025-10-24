namespace Game.Inventory
{
    /// <summary>
    /// Defines a storage driver for saving and loading inventory data.
    /// The implementation can leverage PlayerPrefs, cloud storage, files, or custom backends.
    /// </summary>
    public interface IInventoryPersistence
    {
        InventorySnapshot LoadSnapshot();

        void SaveSnapshot(InventorySnapshot snapshot);
    }
}
