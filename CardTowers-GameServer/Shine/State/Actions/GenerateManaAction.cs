using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State.Actions
{
    public class GenerateManaAction : IDeltaAction<PlayerDelta>
    {
        private Mana mana;

        public GenerateManaAction(Mana mana)
        {
            this.mana = mana;
        }

        public void Execute(IDeltaObject<PlayerDelta> deltaObject, PlayerDelta delta)
        {
            long currentTickTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int previousMana = mana.GetCurrentMana();
            mana.UpdateMana(currentTickTime);
            int newMana = mana.GetCurrentMana();

            delta.GeneratedMana = newMana - previousMana;
        }
    }
}