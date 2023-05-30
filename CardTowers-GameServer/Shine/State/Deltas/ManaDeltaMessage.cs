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
        public float ManaChange { get; set; }

        public ManaDeltaMessage() { }

        public ManaDeltaMessage(string componentId, string gameSessionId, float manaChange)
        {
            GameSessionId = gameSessionId;
            ComponentId = componentId;
            ManaChange = manaChange;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ManaChange);
        }

        public void Deserialize(NetDataReader reader)
        {
            ManaChange = reader.GetFloat();
        }

        public void Handle(NetPeer peer)
        {
            throw new NotImplementedException();
        }
    }
}
