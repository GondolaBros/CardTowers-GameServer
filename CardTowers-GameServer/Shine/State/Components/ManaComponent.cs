using System;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State.Components
{
    using CardTowers_GameServer.Shine.Messages.Interfaces;
    using CardTowers_GameServer.Shine.Models;

    public class ManaComponent : ComponentStateBase
    {
        private Mana mana;

        public ManaComponent(Frequency freq)
            : base(freq)
        {
            mana = new Mana();
        }

        public override void ApplyDelta(IGameMessage delta)
        {
            if (delta is ManaDeltaMessage manaDelta)
            {
                // Negative change is spent mana, positive change is gained mana
                if (manaDelta.ManaChange < 0)
                {
                    if (mana.CanSpendMana(-manaDelta.ManaChange))
                    {
                        mana.SpendMana(-manaDelta.ManaChange);
                    }
                    // else log an error or handle the situation appropriately
                }
                else
                {
                    mana.SetCurrentMana(mana.GetCurrentMana() + manaDelta.ManaChange);
                }
            }
            else
            {
                throw new ArgumentException($"Invalid delta type: {delta.GetType()}");
            }
        }


        public override IGameMessage GenerateDelta()
        {
            long currentTickTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            mana.Update(currentTickTime);

            return new ManaDeltaMessage(ComponentId, GameSessionId, mana.GetCurrentMana());
        }

        public override void Update(long deltaTime)
        {
            // Your game specific logic goes here
            long currentTickTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            mana.Update(currentTickTime);
        }
    }

}

