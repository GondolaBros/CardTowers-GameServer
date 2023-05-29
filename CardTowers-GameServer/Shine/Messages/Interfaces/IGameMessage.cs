using LiteNetLib.Utils;

namespace CardTowers_GameServer.Shine.Messages.Interfaces
{
    public interface IGameMessage : INetworkMessage
    {
        string GameSessionId { get; set; }
        string ComponentId { get; set; }
    }
}
