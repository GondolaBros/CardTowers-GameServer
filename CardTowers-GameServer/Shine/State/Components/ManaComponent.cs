using System;
using CardTowers_GameServer.Shine.Messages.Interfaces;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State.Components
{
    public class ManaComponent : ComponentStateBase
    {
        public Mana ManaModel;

        public ManaComponent(Frequency freq)
            : base(freq)
        {
            ManaModel = new Mana();
        }

        public override void ApplyServerAction(IGameMessage serverAction)
        {
            if (serverAction is ManaDeltaMessage manaDelta)
            {
                // Positive change is gained mana
                ManaModel.SetCurrentMana(ManaModel.GetCurrentMana() + manaDelta.ManaChange);
            }
            else
            {
                throw new ArgumentException($"Invalid server action type: {serverAction.GetType()}");
            }
        }

        public override bool IsValidClientAction(IGameMessage clientAction)
        {
            if (clientAction is ManaDeltaMessage manaDelta)
            {
                // Negative change is spent mana, positive change is gained mana
                if (manaDelta.ManaChange < 0)
                {
                    return ManaModel.CanSpendMana(-manaDelta.ManaChange);
                }
                else
                {
                    // No additional validation needed for positive mana change
                    return true;
                }
            }

            // Invalid client action type
            return false;
        }

        public override void ApplyClientAction(IGameMessage clientAction)
        {
            if (clientAction is ManaDeltaMessage manaDelta)
            {
                // Negative change is spent mana, positive change is gained mana
                if (manaDelta.ManaChange < 0)
                {
                    ManaModel.SpendMana(-manaDelta.ManaChange);
                }
                else
                {
                    ManaModel.SetCurrentMana(ManaModel.GetCurrentMana() + manaDelta.ManaChange);
                }
            }
            else
            {
                throw new ArgumentException($"Invalid client action type: {clientAction.GetType()}");
            }
        }

        public override void HandleInvalidClientAction(IGameMessage clientAction)
        {
            // Handle invalid client actions here
            // For example, you might send a message to the client informing them that their action was invalid
        }

        public override IGameMessage GenerateServerAction()
        {
            return new ManaDeltaMessage(ComponentId, GameSessionId, this.ManaModel.GetCurrentMana());
        }


        public override void FrequencyUpdate(int intervals)
        {
            // Called after a frequency update
            Console.WriteLine("AccumulatedDeltaTime: " + this.AccumulatedDeltaTime);
        }


        public override void Update(long deltaTime)
        {
            this.ManaModel.Update(deltaTime);
        }
    }
}
