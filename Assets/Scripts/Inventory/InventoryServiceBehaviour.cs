using UnityEngine;

namespace Game.Inventory
{
    /// <summary>
    /// Scene binding that wires a persistence driver (if supplied) and initialises the
    /// inventory service lifecycle. Attach this component to a bootstrap GameObject in
    /// the first loaded scene.
    /// </summary>
    public class InventoryServiceBehaviour : MonoBehaviour
    {
        [Tooltip("Optional persistence driver that satisfies IInventoryPersistence.")]
        [SerializeField]
        private MonoBehaviour persistenceDriver;

        private IInventoryPersistence persistenceInstance;

        private void Awake()
        {
            if (persistenceDriver != null)
            {
                persistenceInstance = persistenceDriver as IInventoryPersistence;
                if (persistenceInstance == null)
                {
                    Debug.LogWarning($"{persistenceDriver.name} must implement IInventoryPersistence to be used by the InventoryService.");
                }
            }

            if (persistenceInstance != null)
            {
                InventoryService.Instance.ConfigurePersistence(persistenceInstance);
            }

            InventoryService.Instance.Load();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                InventoryService.Instance.Save();
            }
        }

        private void OnApplicationQuit()
        {
            InventoryService.Instance.Save();
        }
    }
}
