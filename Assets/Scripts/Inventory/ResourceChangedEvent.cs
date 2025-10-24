namespace Game.Inventory
{
    /// <summary>
    /// Describes the delta applied to a resource entry.
    /// </summary>
    public readonly struct ResourceChangedEvent
    {
        public ResourceType Type { get; }

        public int PreviousValue { get; }

        public int NewValue { get; }

        public int Delta { get; }

        public bool IsGain => Delta > 0;

        public bool IsSpend => Delta < 0;

        public ResourceChangedEvent(ResourceType type, int previousValue, int newValue)
        {
            Type = type;
            PreviousValue = previousValue;
            NewValue = newValue;
            Delta = newValue - previousValue;
        }
    }
}
