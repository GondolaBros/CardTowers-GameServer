using System;

namespace CardTowers_GameServer.Shine.State
{
    public interface IGameState<TDelta, TAction> : IDeltaObject<TDelta>
 where TDelta : Delta
 where TAction : IDeltaAction<TDelta>
    {
        // ApplyDeltaAction removed from here
        IGameStateSnapshot<TDelta> CreateSnapshot();
        void RestoreFromSnapshot(IGameStateSnapshot<TDelta> snapshot);
    }

}
