using System;
namespace CardTowers_GameServer.Shine.State
{
    public interface IGameState
    {
        void ApplyDeltaState(DeltaState deltaState);
        DeltaState GetDeltaState(IGameState oldState);
    }
}