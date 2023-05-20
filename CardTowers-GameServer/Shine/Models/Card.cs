using System;

namespace CardTowers_GameServer.Shine.Models
{
    public class Card
    {
        public int ElixirCost;
        public string CardName;
        public CardPlacementAction PlacementAction;
    }

    public delegate void CardPlacementAction(Card card, ref GameMap playerBoard);
}

