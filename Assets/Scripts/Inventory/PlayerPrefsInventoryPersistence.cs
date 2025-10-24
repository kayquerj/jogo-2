using UnityEngine;

namespace Game.Inventory
{
    /// <summary>
    /// Lightweight persistence driver that serialises inventory data to PlayerPrefs.
    /// Ideal for prototypes or mobile builds where cloud save is not yet required.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerPrefsInventoryPersistence : MonoBehaviour, IInventoryPersistence
    {
        private const string DefaultPrefsKey = "game.inventory";

        [SerializeField]
        private string playerPrefsKey = DefaultPrefsKey;

        public InventorySnapshot LoadSnapshot()
        {
            if (!PlayerPrefs.HasKey(playerPrefsKey))
            {
                return new InventorySnapshot();
            }

            var json = PlayerPrefs.GetString(playerPrefsKey);
            if (string.IsNullOrEmpty(json))
            {
                return new InventorySnapshot();
            }

            try
            {
                return JsonUtility.FromJson<InventorySnapshot>(json) ?? new InventorySnapshot();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to parse inventory snapshot from PlayerPrefs: {exception.Message}");
                return new InventorySnapshot();
            }
        }

        public void SaveSnapshot(InventorySnapshot snapshot)
        {
            if (snapshot == null)
            {
                PlayerPrefs.DeleteKey(playerPrefsKey);
                return;
            }

            var json = JsonUtility.ToJson(snapshot);
            PlayerPrefs.SetString(playerPrefsKey, json);
            PlayerPrefs.Save();
        }
    }
}
