using System;

namespace Game.Inventory
{
    /// <summary>
    /// Defines the available resource categories handled by the inventory system.
    /// Extend this enum to add support for new resource types.
    /// </summary>
    [Serializable]
    public enum ResourceType
    {
        Wood,
        Stone,
        Fiber,
        Food,
        Gold
    }
}
