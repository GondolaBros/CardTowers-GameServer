using CardTowers_GameServer.Shine.Data;
using CardTowers_GameServer.Shine.Data.Entities;
using CardTowers_GameServer.Shine.Matchmaking;
using LiteNetLib;

namespace CardTowers_GameServer.Shine.Models
{
    public class Player
    {
        public NetPeer Peer { get; private set; }
        public PlayerEntity Entity { get; private set; }

        public Player(NetPeer peer, PlayerEntity playerEntity)
        {
            this.Peer = peer;
            this.Entity = playerEntity;
        }
    }
}

