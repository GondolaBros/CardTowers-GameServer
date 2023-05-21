using System;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.State.Deltas;
using CardTowers_GameServer.Shine.Util;

namespace CardTowers_GameServer.Shine.State
{
    public class PlayerState : DeltaObjectBase<PlayerDelta>, IGameState<PlayerDelta, IDeltaAction<PlayerDelta>>
    {
        // Player state properties go here.

        protected override PlayerDelta CreateDelta()
        {
            // Logic to create a delta from the current and previous state goes here.
            // For now, return an empty PlayerDelta.
            return new PlayerDelta();
        }

        public void ApplyDeltaAction(IDeltaAction<PlayerDelta> deltaAction)
        {
            // Logic to apply a delta action goes here.
            deltaAction.Execute(this);
        }

        public IGameStateSnapshot<PlayerDelta> CreateSnapshot()
        {
            // Logic to create a snapshot goes here.
            // For now, return a snapshot that contains the current delta.
            return new PlayerStateSnapshot { PlayerDelta = currentDelta };
        }

        public void RestoreFromSnapshot(IGameStateSnapshot<PlayerDelta> snapshot)
        {
            // Logic to restore state from a snapshot goes here.
            ApplyDelta(snapshot.GetDelta());
        }
    }
}
