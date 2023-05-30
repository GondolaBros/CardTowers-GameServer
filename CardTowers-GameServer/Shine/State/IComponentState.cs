using CardTowers_GameServer.Shine.Messages.Interfaces;

namespace CardTowers_GameServer.Shine.State
{
    public interface IComponentState
    {
        Frequency Frequency { get; }
        string ComponentId { get; set; }
        string GameSessionId { get; set; }
        long AccumulatedDeltaTime { get; }
        IGameMessage GenerateServerAction();
        IGameMessage? GetCurrentServerAction();
        void ResetCurrentServerAction();
        void FrequencyUpdate(int intervals);
        void Update(long deltaTime);
        void ProcessUpdate(long deltaTime);
        void ApplyServerAction(IGameMessage serverAction);
        bool IsValidClientAction(IGameMessage clientAction);
        void HandleInvalidClientAction(IGameMessage clientAction);
        void ApplyClientAction(IGameMessage clientAction);
    }
}