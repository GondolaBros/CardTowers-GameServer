using System;

namespace CardTowers_GameServer.Shine.Entities
{
    public class Card
    {
        public string CardName;
        public CardPlacementAction PlacementAction;
    }

    public delegate void CardPlacementAction(Card card, ref GameMap playerBoard);
}

