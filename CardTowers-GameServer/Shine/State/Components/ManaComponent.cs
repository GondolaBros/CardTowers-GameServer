using System;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State.Components
{
    public class ManaComponent : ComponentStateBase<ManaDelta>
    {
        private Mana mana;

        public ManaComponent(Frequency freq) : base(freq)
        {
            mana = new Mana();
        }


        public override void ApplyDelta(ManaDelta delta)
        {
            // Negative change is spent mana, positive change is gained mana
            if (delta.ManaChange < 0)
            {
                mana.SpendMana(-delta.ManaChange);
            }
            else
            {
                mana.SetCurrentMana(mana.GetCurrentMana() + delta.ManaChange);
            }
        }


        public override ManaDelta GenerateDelta()
        {
            ManaDelta delta = new ManaDelta();

            // Generate a delta of mana generation per second 
            // or generation per frequency based on your game's need
            // This can be zero if no mana was generated
            long currentTickTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            mana.Update(currentTickTime);

            delta.ManaChange = mana.GetCurrentMana();

            return delta;
        }

        public override void Update(long deltaTime)
        {
            // Your game specific logic goes here
            // For example, you could check if there are any entities that should generate mana and call their generate mana method
        }
    }

}

