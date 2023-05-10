using System;
using CardTowers_GameServer.Shine.Network;

namespace CardTowers_GameServer.Shine.Matchmaking
{
    public class MatchmakingEntry
    {
        public Connection Connection { get; set; }
        public MatchmakingParameters Parameters { get; set; }

        public MatchmakingEntry(Connection connection, MatchmakingParameters parameters)
        {
            Connection = connection;
            Parameters = parameters;
        }
    }
}

