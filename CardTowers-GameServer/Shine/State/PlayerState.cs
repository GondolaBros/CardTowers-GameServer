using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State.Actions;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State
{
    public class PlayerState : DeltaObjectBase<PlayerDelta>, IGameState<PlayerDelta, IDeltaAction<PlayerDelta>>
    {
        public Mana Mana { get; private set; }
        public GameMap Map { get; private set; }

        public PlayerState()
        {
            Mana = new Mana();
            Map = new GameMap();
            RegisterDeltaAction<GenerateManaAction>(new GenerateManaAction(Mana));
            // Register other delta actions as needed.
        }


        protected override PlayerDelta CreateDelta()
        {
            // Create a delta from the current state
            PlayerDelta delta = new PlayerDelta();

            // Update the delta to reflect the current mana
            delta.GeneratedMana = Mana.GetCurrentMana();

            return delta;
        }



        public IGameStateSnapshot<PlayerDelta> CreateSnapshot()
        {
            // Return a snapshot that contains the current delta and the current mana.
            return new PlayerStateSnapshot { PlayerDelta = currentDelta, Mana = Mana.GetCurrentMana() };
        }


        public void RestoreFromSnapshot(IGameStateSnapshot<PlayerDelta> snapshot)
        {
            // Get the state stored in the snapshot.
            PlayerStateSnapshot playerStateSnapshot = (PlayerStateSnapshot)snapshot;

            // Apply the stored delta to this state.
            ApplyDelta(playerStateSnapshot.GetDelta());

            // Restore the mana to the value stored in the snapshot.
            Mana.SetCurrentMana(playerStateSnapshot.Mana);
        }
    }
}
