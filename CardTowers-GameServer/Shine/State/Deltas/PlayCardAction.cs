using CardTowers_GameServer.Shine.Models;

namespace CardTowers_GameServer.Shine.State
{
    public class PlayCardAction : DeltaState
    {
        private readonly Card _card;

        public PlayCardAction(Card card)
        {
            _card = card;
        }

        public override void Apply(IGameState state)
        {
            PlayerState playerState = (PlayerState)state;

            if (playerState.Elixir >= _card.ElixirCost)
            {
                // Subtract the card's elixir cost from the player's current elixir
                playerState.Elixir -= _card.ElixirCost;

                // TODO: Add the card to the player's map
            }
            else
            {
                throw new Exception("Not enough elixir to play this card.");
            }
        }
    }
}
