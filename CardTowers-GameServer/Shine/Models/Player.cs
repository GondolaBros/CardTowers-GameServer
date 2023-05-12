using CardTowers_GameServer.Shine.Data;
using CardTowers_GameServer.Shine.Network;
using LiteNetLib;

namespace CardTowers_GameServer.Shine.Models
{
    public class Player
    {
        public NetPeer Peer { get; private set; }
        public PlayerData Data { get; private set; }
        public GameMap GameMap { get; private set; }
        public Deck Deck { get; private set; }

        public Player(NetPeer peer, PlayerData playerData)
        {
            this.Peer = peer;
            this.Data = playerData;
        }
    }
}

