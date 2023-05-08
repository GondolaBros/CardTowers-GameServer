using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace CardTowers_GameServer.Shine.Interfaces
{
    public interface IHandledMessage : INetSerializable
    {
        void Handle(NetPeer peer);
    }
}
