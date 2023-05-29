using System;
using CardTowers_GameServer.Shine.Messages.Interfaces;

namespace CardTowers_GameServer.Shine.State
{
    public abstract class ComponentStateBase : IComponentState
    {
        public Frequency Frequency { get; private set; }
        public string ComponentId { get; set; }
        public string GameSessionId { get; set; }
            
        protected IGameMessage? currentDelta;

        public abstract IGameMessage GenerateDelta();
        public abstract void Update(long deltaTime);
        public abstract void ApplyDelta(IGameMessage delta);

        public event Action<IGameMessage> OnDeltaCreated;

        protected ComponentStateBase(Frequency frequency)
        {
            Frequency = frequency;
        }

        public void ProcessUpdate(long deltaTime)
        {
            // Call the component-specific update method
            Update(deltaTime);

            // If the component's frequency isn't event-based, try to generate and apply a delta
            if (Frequency != Frequency.EventBased && ShouldGenerateDelta(deltaTime))
            {
                currentDelta = GenerateDelta();

                OnDeltaCreated?.Invoke(currentDelta);

                // Validate the delta before applying
                if (IsValidDelta(currentDelta))
                {
                    ApplyDelta(currentDelta);
                }
                else
                {
                    HandleInvalidDelta(currentDelta);
                }
            }
        }

        public bool ShouldGenerateDelta(long deltaTime)
        {
            // If the elapsed time is greater or equal to the frequency, it's time to generate a delta
            return deltaTime >= (long)Frequency;
        }

        public virtual IGameMessage CreateNewDeltaInstance()
        {
            // This function can be overridden in child classes to create a new instance of the specific IGameMessage type.
            throw new NotImplementedException("CreateNewDeltaInstance should be implemented in a child class.");
        }

        public IGameMessage? GetCurrentDelta()
        {
            return currentDelta;
        }

        // Validate the delta before applying
        public virtual bool IsValidDelta(IGameMessage delta)
        {
            // Implement validation logic specific to the component
            // Return true if the delta is valid; otherwise, return false
            // Perform necessary checks based on the component's requirements
            // Example: Check if the delta falls within valid bounds, respects constraints, or complies with specific rules
            return true;
        }

        // Handle an invalid delta
        public virtual void HandleInvalidDelta(IGameMessage delta)
        {
            // Implement handling logic for invalid deltas
            // This can include logging, notifying, or taking corrective actions based on your game's requirements
            // Example: Log an error, notify relevant systems or components, or trigger specific actions to recover from the error gracefully
        }
    }
}
