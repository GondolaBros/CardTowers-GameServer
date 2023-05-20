namespace CardTowers_GameServer.Shine.State
{
    public class SpendElixirAction : DeltaState
    {
        private int Amount { get; set; }

        public SpendElixirAction(int amount)
        {
            Amount = amount;
        }


        public override void Apply(IGameState state)
        {
            var playerState = state as PlayerState;
            if (playerState == null)
            {
                throw new InvalidOperationException("DeltaState can only be applied to a PlayerState instance.");
            }

            playerState.Elixir -= Amount;

            // Ensure elixir does not go below zero
            if (playerState.Elixir < 0)
            {
                playerState.Elixir = 0;
            }
        }
    }

}


