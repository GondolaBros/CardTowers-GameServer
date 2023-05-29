using LiteNetLib.Utils;
using CardTowers_GameServer.Shine.Messages.Interfaces;
using CardTowers_GameServer.Shine.State;

namespace CardTowers_GameServer.Shine.State
{
    public interface IGameMessageSerializer
    {
        void Serialize(IGameMessage message, NetDataWriter writer);
        IGameMessage Deserialize(NetDataReader reader);
        GameMessageType GetMessageType(IGameMessage message);
    }
}
