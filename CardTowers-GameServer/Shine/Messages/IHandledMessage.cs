using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace CardTowers_GameServer.Shine.Messages
{
    public interface IHandledMessage : INetSerializable
    {
        void Handle(NetPeer peer);
    }
}
