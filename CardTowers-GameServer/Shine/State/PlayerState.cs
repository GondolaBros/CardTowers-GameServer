using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State.Actions;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State
{
    public class PlayerState : DeltaObjectBase<PlayerDelta>, IGameState<PlayerDelta, IDeltaAction<PlayerDelta>>
    {
        public Mana Mana { get; private set; }

        public PlayerState()
        {
            Mana = new Mana();
            RegisterDeltaAction<GenerateManaAction>(new GenerateManaAction(Mana));
            // Register other delta actions as needed.
        }


        protected override PlayerDelta CreateDelta()
        {
            // Logic to create a delta from the current and previous state goes here.
            // For now, return an empty PlayerDelta.
            return new PlayerDelta();
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