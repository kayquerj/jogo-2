using UnityEngine;

namespace Harvesting
{
    public class ResourcePickup : MonoBehaviour
    {
        [SerializeField]
        private ResourceYield[] yields;

        public ResourceYield[] Yields => yields;

        public void SetYields(ResourceYield[] newYields)
        {
            if (newYields == null)
            {
                yields = null;
                return;
            }

            yields = new ResourceYield[newYields.Length];
            newYields.CopyTo(yields, 0);
        }
    }
}
