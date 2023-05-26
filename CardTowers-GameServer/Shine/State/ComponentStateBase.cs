using CardTowers_GameServer.Shine.State;

public abstract class ComponentStateBase<TDelta> : IComponentState<TDelta> where TDelta : IDelta, new()
{
    // Flag indicating if the component is event-driven
    private bool isEventDriven;

    // Queue to store delta history
    private Queue<TDelta> deltaHistory;

    // Event triggered when a delta is generated
    public event Action<TDelta>? DeltaGenerated;

    public abstract void ApplyDelta(TDelta delta); // Abstract method to apply a delta to the component
    public abstract TDelta GenerateDelta(); // Abstract method to generate a new delta
    public abstract void Update(long deltaTime); // Abstract method to update the components game logic

    // Frequency at which deltas are generated
    public virtual Frequency Frequency { get; private set; }

    public ComponentStateBase(Frequency syncFrequency)
    {
        Frequency = syncFrequency;

        // Check if the component is event-driven
        isEventDriven = Frequency == Frequency.EventBased;

        // Create a new queue for delta history
        deltaHistory = new Queue<TDelta>();
    }

    public void InternalUpdate(long deltaTime)
    {
        BaseUpdate(deltaTime);
    }

    private void BaseUpdate(long deltaTime)
    {
        // Call the specific update logic for the component
        Update(deltaTime);

        // Skip delta generation if the component is event-driven
        if (isEventDriven)
            return;

        // Check if enough time has passed to generate a new delta
        if (ShouldGenerateDelta(deltaTime))
        {
            // Generate a new delta and store it in the snapshot
            TDelta delta = GenerateDelta();

            // Trigger the DeltaGenerated event
            DeltaGenerated?.Invoke(delta);

            // Apply the generated delta to the component
            ApplyDelta(delta);

            // Add the delta to the history
            deltaHistory.Enqueue(delta);
        }
    }

    public virtual bool IsStateConsistent()
    {
        // Dummy return. Implement game-specific logic here.
        return true;
    }

    public bool ShouldGenerateDelta(long deltaTime)
    {
        // Check if enough time has passed since the last delta generation
        return deltaTime >= (long)Frequency;
    }

    public virtual IDelta CreateNewDeltaInstance()
    {
        return new TDelta();
    }

    // Get the delta history of the component
    public IEnumerable<TDelta> GetDeltaHistory()
    {
        return deltaHistory;
    }

    // Clear the delta history of the component
    public void ClearDeltaHistory()
    {
        deltaHistory.Clear();
    }

    // Apply a series of deltas to the component
    public void ApplyDeltaHistory(IEnumerable<TDelta> deltas)
    {
        foreach (var delta in deltas)
        {
            ApplyDelta(delta);
        }
    }
}
