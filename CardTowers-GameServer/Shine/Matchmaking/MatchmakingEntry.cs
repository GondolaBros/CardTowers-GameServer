using System;
using CardTowers_GameServer.Shine.Network;
using LiteNetLib;

namespace CardTowers_GameServer.Shine.Matchmaking
{
    public class MatchmakingEntry
    {
        public NetPeer Peer { get; private set; }
        public MatchmakingParameters Parameters { get; private set; }

        public MatchmakingEntry(NetPeer peer, MatchmakingParameters parameters)
        {
            this.Peer = peer;
            this.Parameters = parameters;
        }
    }
}

