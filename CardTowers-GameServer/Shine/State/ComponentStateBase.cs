using CardTowers_GameServer.Shine.Messages.Interfaces;
using CardTowers_GameServer.Shine.State;
using System;

public abstract class ComponentStateBase : IComponentState
{
    public Frequency Frequency { get; private set; }
    public string ComponentId { get; set; }
    public string GameSessionId { get; set; }

    protected IGameMessage? currentServerAction;
    public long AccumulatedDeltaTime { get; private set; } // Accumulated time between updates

    public event Action<IGameMessage> OnServerActionCreated;

    protected ComponentStateBase(Frequency frequency)
    {
        Frequency = frequency;
        AccumulatedDeltaTime = 0;
    }

    public void ProcessUpdate(long deltaTime)
    {
        AccumulatedDeltaTime += deltaTime; // Accumulate the elapsed time

        // Calculate the number of frequency intervals that have passed
        int intervalsPassed = (int)(AccumulatedDeltaTime / (long)Frequency);

        if (intervalsPassed > 0)
        {
            AccumulatedDeltaTime -= intervalsPassed * (long)Frequency; // Deduct the passed intervals from accumulated time

            // Call the frequency-based update method with the number of intervals passed
            FrequencyUpdate(intervalsPassed);

            // Generate a server action only if the current server action is null
            if (currentServerAction == null)
            {
                currentServerAction = GenerateServerAction();
                OnServerActionCreated?.Invoke(currentServerAction);
            }

            // Apply the server action
            ApplyServerAction(currentServerAction);
        }

        // Call the regular update method with the current delta time
        Update(deltaTime);
    }


    public void ResetCurrentServerAction() => currentServerAction = null;

    public abstract void Update(long deltaTime);
    public abstract void FrequencyUpdate(int intervals);

    public abstract IGameMessage GenerateServerAction();
    public abstract void ApplyServerAction(IGameMessage serverAction);
    public virtual bool IsValidClientAction(IGameMessage clientAction) => true;
    public virtual void HandleInvalidClientAction(IGameMessage clientAction) { }
    public abstract void ApplyClientAction(IGameMessage clientAction);
    public IGameMessage? GetCurrentServerAction() => currentServerAction;
}
