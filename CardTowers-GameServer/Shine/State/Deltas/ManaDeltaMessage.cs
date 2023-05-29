using CardTowers_GameServer.Shine.Messages.Interfaces;
using CardTowers_GameServer.Shine.State;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace CardTowers_GameServer.Shine.State.Deltas
{
    public class ManaDeltaMessage : IGameMessage
    {
        public GameMessageType MessageType { get; set; } = GameMessageType.Mana;
        public string GameSessionId { get; set; }
        public string ComponentId { get; set; }
        public int ManaChange { get; set; }

        public ManaDeltaMessage(string componentId, string gameSessionId, int manaChange)
        {
            GameSessionId = gameSessionId;
            ComponentId = componentId;
            ManaChange = manaChange;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GameSessionId);
            writer.Put(ComponentId);
            writer.Put(ManaChange);
        }

        public void Deserialize(NetDataReader reader)
        {
            GameSessionId = reader.GetString();
            ComponentId = reader.GetString();
            ManaChange = reader.GetInt();
        }

        public void Handle(NetPeer peer)
        {
            throw new NotImplementedException();
        }
    }
}
