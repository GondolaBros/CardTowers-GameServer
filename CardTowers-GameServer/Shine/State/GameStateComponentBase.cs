using CardTowers_GameServer.Shine.State;

public abstract class GameStateComponentBase<TDelta> : IGameStateComponent<TDelta>, IDeltaComponent where TDelta : IDelta, new()
{
    // Timestamp of the last delta generation
    private long lastDeltaGenerationTime;

    // Flag indicating if the component is event-driven
    private bool isEventDriven;

    // Queue to store delta history
    private Queue<TDelta> deltaHistory;

    // Frequency at which deltas are generated
    public abstract Frequency Frequency { get; }

    // Unique identifier for the component
    public string ComponentID { get; }

    // Event triggered when a delta is generated
    public event Action<TDelta>? DeltaGenerated;

    // Abstract method to apply a delta to the component
    public abstract void ApplyDelta(TDelta delta);

    // Abstract method to generate a new delta
    public abstract TDelta GenerateDelta();

    // Abstract method to update the components game logic
    public abstract void Update(long deltaTime);

    public GameStateComponentBase(string componentID)
    {
        ComponentID = componentID;

        // Initialize the timestamp with the current time
        lastDeltaGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Check if the component is event-driven
        isEventDriven = Frequency == Frequency.EventBased;

        // Create a new queue for delta history
        deltaHistory = new Queue<TDelta>();
    }


    
    public void BaseUpdate(long deltaTime)
    {
        // Call the specific update logic for the component
        Update(deltaTime);

        // Skip delta generation if the component is event-driven
        if (isEventDriven)
            return;

        // Get the current timestamp
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Check if enough time has passed to generate a new delta
        if (ShouldGenerateDelta(currentTimestamp))
        {
            // Generate a new delta
            TDelta delta = GenerateDelta();

            // Update the last delta generation time
            lastDeltaGenerationTime = currentTimestamp;

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

    // Generates a snapshot of the current state
    public virtual GameStateSnapshot<IDelta> GenerateSnapshot()
    {
        // This simply wraps the current state (the result of GenerateDelta) in a GameStateSnapshot
        return new GameStateSnapshot<IDelta>(GenerateDelta());
    }


    public bool ShouldGenerateDelta(long currentTimestamp)
    {
        // Check if enough time has passed since the last delta generation
        return currentTimestamp - lastDeltaGenerationTime >= (long)Frequency;
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

    // Implement the GenerateDelta method from IDeltaComponent interface
    IDelta IDeltaComponent.GenerateDelta()
    {
        return GenerateDelta();
    }

    // Implement the ApplyDelta method from IDeltaComponent interface
    void IDeltaComponent.ApplyDelta(IDelta delta)
    {
        ApplyDelta((TDelta)delta);
    }
}
