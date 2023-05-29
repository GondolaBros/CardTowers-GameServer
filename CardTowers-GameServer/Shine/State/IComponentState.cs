using CardTowers_GameServer.Shine.Messages.Interfaces;

namespace CardTowers_GameServer.Shine.State
{
    public interface IComponentState
    {
        Frequency Frequency { get; }
        string ComponentId { get; set; }
        string GameSessionId { get; set; }
        IGameMessage CreateNewDeltaInstance();
        IGameMessage GenerateDelta();
        IGameMessage? GetCurrentDelta();
        void Update(long deltaTime);
        void ProcessUpdate(long deltaTime);
        void ApplyDelta(IGameMessage delta);
        bool ShouldGenerateDelta(long deltaTime);
        bool IsValidDelta(IGameMessage delta);
        void HandleInvalidDelta(IGameMessage delta);
    }
}
