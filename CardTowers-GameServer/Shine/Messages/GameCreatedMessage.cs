using System;
using CardTowers_GameServer.Shine.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace CardTowers_GameServer.Shine.Messages
{
	public class GameCreatedMessage : IHandledMessage
	{
        public Guid Id { get; set; }


        public void Deserialize(NetDataReader reader)
        {
            throw new NotImplementedException();
        }

        public void Serialize(NetDataWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Handle(NetPeer peer)
        {
            throw new NotImplementedException();
        }
    }
}

