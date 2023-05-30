using System;
using System.Collections.Generic;
using LiteNetLib.Utils;
using CardTowers_GameServer.Shine.Messages.Interfaces;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State
{
    public class GameMessageSerializer : IGameMessageSerializer
    {
        private Dictionary<GameMessageType, Type> messageTypes = new Dictionary<GameMessageType, Type>();

        public GameMessageSerializer()
        {
            // Add mappings for all IGameMessage types.
            messageTypes[GameMessageType.Mana] = typeof(ManaDeltaMessage);
            // Add more mappings here as necessary...
        }

        public void Serialize(IGameMessage message, NetDataWriter writer)
        {
            // Write the message type, game session ID, and component ID first
            writer.Put((byte)GetMessageType(message));
            writer.Put(message.GameSessionId);
            writer.Put(message.ComponentId);

            // Then call the message's Serialize method
            message.Serialize(writer);
        }

        public IGameMessage Deserialize(NetDataReader reader)
        {
            // First read the message type, game session ID, and component ID
            GameMessageType messageType = (GameMessageType)reader.GetByte();
            string gameSessionId = reader.GetString();
            string componentId = reader.GetString();

            // Then use the messageType to create a new instance of the correct type
            if (messageTypes.TryGetValue(messageType, out var type))
            {
                IGameMessage message = (IGameMessage)Activator.CreateInstance(type);
                message.GameSessionId = gameSessionId;
                message.ComponentId = componentId;
                message.Deserialize(reader);
                return message;
            }
            else
            {
                throw new Exception($"Unknown message type: {messageType}");
            }
        }

        public GameMessageType GetMessageType(IGameMessage message)
        {
            // Use the actual type of the message to look up the GameMessageType
            foreach (var pair in messageTypes)
            {
                if (pair.Value == message.GetType())
                {
                    return pair.Key;
                }
            }

            throw new Exception($"Unknown message type: {message.GetType()}");
        }
    }
}