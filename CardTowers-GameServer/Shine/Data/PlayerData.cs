using System;
using CardTowers_GameServer.Shine.Models;

namespace CardTowers_GameServer.Shine.Data
{
	public class PlayerData
	{
        public string Username { get; set; }

        public Deck Deck { get; set; }

        public void LoadFromDatabase(string accountId)
        {
           // this.Username = db.account.username
           // this.EloRating = db.game.skill
           // this.SelectedDeck = decklibary.getSelected
        }
    }
}

