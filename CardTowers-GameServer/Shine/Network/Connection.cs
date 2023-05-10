using LiteNetLib;
using System;

namespace CardTowers_GameServer.Shine.Network
{
    public class Connection
    {
        public int Id { get; private set; }
        public NetPeer Peer { get; private set; }

        public Connection(NetPeer peer)
        {
            Id = peer.Id;
            Peer = peer;
        }
    }
}

