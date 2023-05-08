using System;
namespace CardTowers_GameServer.Shine.Data
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

