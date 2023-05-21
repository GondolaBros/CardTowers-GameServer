using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State.Actions
{
    public class SpendManaAction : IDeltaAction<PlayerDelta>
    {
        public int ElixirSpent { get; }

        public SpendManaAction(int elixirSpent)
        {
            ElixirSpent = elixirSpent;
        }

        public void Execute(IDeltaObject<PlayerDelta> deltaObject, PlayerDelta delta)
        {
            delta.Mana.SpendMana(ElixirSpent);
            deltaObject.ApplyDelta(delta);
        }
    }
}
