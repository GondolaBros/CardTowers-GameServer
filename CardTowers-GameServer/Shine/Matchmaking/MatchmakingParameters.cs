using System;
using LiteNetLib;

namespace CardTowers_GameServer.Shine.Matchmaking
{
    public class MatchmakingParameters
    {
        // eventually replace this based on our actual matchmaking server
        // connection. ideally some sort of https based service
        //public NetPeer Peer { get; private set; }
        public int Id { get; set; }
        public int EloRating { get; set; }
        public string Username { get; set; }

        // Game mode?

        // Ping ?

        // Geographic location properties
    }
}

