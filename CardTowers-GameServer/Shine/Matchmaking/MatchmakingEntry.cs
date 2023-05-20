using System;
using CardTowers_GameServer.Shine.Models;
using LiteNetLib;

namespace CardTowers_GameServer.Shine.Matchmaking
{
    public class MatchmakingEntry
    {
        public bool IsMatched { get; set; }

        //public Player Player { get; private set; }
        public MatchmakingParameters Parameters { get; private set; }

        public MatchmakingEntry(MatchmakingParameters parameters)
        {
            //this.Player = player;
            this.Parameters = parameters;
        }
    }
}

