using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace CardTowers_GameServer.Shine.Messages.Interfaces
{
    public interface INetworkMessage : INetSerializable
    {
        void Handle(NetPeer peer);
    }
}
