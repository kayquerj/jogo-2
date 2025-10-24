using System;

namespace Harvesting
{
    [Serializable]
    public struct ResourceYield
    {
        public string resourceId;
        public int amount;

        public ResourceYield(string resourceId, int amount)
        {
            this.resourceId = resourceId;
            this.amount = amount;
        }
    }
}
