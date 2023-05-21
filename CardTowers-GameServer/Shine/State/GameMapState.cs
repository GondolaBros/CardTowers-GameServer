using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State
{
    public class GameMapState : DeltaObjectBase<GameMapDelta>, IGameState<GameMapDelta, IDeltaAction<GameMapDelta>>
    {
        public GameMapState()
        {
            //RegisterDeltaAction<UpdateTileAction>(new UpdateTileAction());
            // Register other delta actions for the game map as needed.
        }

        protected override GameMapDelta CreateDelta()
        {
            // Logic to create a delta from the current and previous state goes here.
            // For now, return an empty GameMapDelta.
            return new GameMapDelta();
        }

        public IGameStateSnapshot<GameMapDelta> CreateSnapshot()
        {
            // Logic to create a snapshot goes here.
            // For now, return a snapshot that contains the current delta.
            //return new GameMapStateSnapshot { GameMapDelta = currentDelta };

            return null;
        }

        public void RestoreFromSnapshot(IGameStateSnapshot<GameMapDelta> snapshot)
        {
            // Logic to restore state from a snapshot goes here.
            ApplyDelta(snapshot.GetDelta());
        }
    }
}
