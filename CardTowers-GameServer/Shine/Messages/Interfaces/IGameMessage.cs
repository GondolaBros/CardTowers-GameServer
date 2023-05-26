using System;
namespace CardTowers_GameServer.Shine.Messages.Interfaces
{
    public interface IGameMessage : INetworkMessage
    {
        string GameSessionId { get; }
    }
}

