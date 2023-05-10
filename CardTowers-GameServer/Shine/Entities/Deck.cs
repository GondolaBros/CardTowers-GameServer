using System;
namespace CardTowers_GameServer.Shine.Entities
{
    using System.Collections;
    using System.Collections.Generic;

    public class Deck
    {
        private const int MAX_CARDS_IN_DECK = 25;
        private List<Card> cards = new List<Card>();
        private string deckName;

        public Card[] Cards
        {
            get { return cards.ToArray(); }
        }

        public Deck() { }

        public void AddCard(Card card)
        {
            List<Card> updatedCards = new List<Card>(cards);
            updatedCards.Add(card);

            if (Deck.IsValid(updatedCards.ToArray()))
            {
                cards.Add(card);
            }
        }

        public void ReplaceCard(int index, Card card)
        {
            if (cards[index] != null)
            {
                List<Card> updatedCards = new List<Card>(cards);
                updatedCards[index] = card;

                if (Deck.IsValid(updatedCards.ToArray()))
                {
                    cards[index] = card;
                }
            }
        }

        public void Shuffle()
        {
            System.Random random = new System.Random();

            int n = cards.Count;
            for (int i = 0; i < n; i++)
            {
                int r = i + random.Next(n - i);
                Card temp = cards[i];
                cards[i] = cards[r];
                cards[r] = temp;
            }
        }

        public bool IsValid()
        {
            return Deck.IsValid(this);
        }

        public static bool IsValid(Deck deck)
        {
            return Deck.IsValid(deck.Cards);
        }

        public static bool IsValid(Card[] cards)
        {
            return cards.Length <= MAX_CARDS_IN_DECK;
        }
    }

}

