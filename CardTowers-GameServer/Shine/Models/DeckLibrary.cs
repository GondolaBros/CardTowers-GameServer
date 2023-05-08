using System;

namespace CardTowers_GameServer.Shine.Models
{
    public class DeckLibrary
    {
        private List<Deck> decks = new List<Deck>();

        public Deck[] Decks
        {
            get { return decks.ToArray(); }
        }

        public void AddDeck(Deck deck)
        {
            if (deck.IsValid())
            {
                decks.Add(deck);
            }
        }

        public void RemoveDeck(int index)
        {
            if (decks[index] != null)
            {
                decks.RemoveAt(index);
            }
        }
    }

}

