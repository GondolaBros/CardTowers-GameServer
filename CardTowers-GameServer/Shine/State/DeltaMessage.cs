using System;
using CardTowers_GameServer.Shine.Messages;
using CardTowers_GameServer.Shine.Messages.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace CardTowers_GameServer.Shine.State
{
    public class DeltaMessage<TDelta> : IGameMessage where TDelta : IDelta
    {
        public TDelta Delta { get; set; }

        public string GameSessionId => throw new NotImplementedException();

        public void Deserialize(NetDataReader reader)
        {
            //throw new NotImplementedException();
        }

        public void Handle(NetPeer peer)
        {
            // You won't actually handle delta messages here.
            // Instead, they'll be routed to the game session.
            //throw new NotImplementedException();
        }

        public void Serialize(NetDataWriter writer)
        {
            //throw new NotImplementedException();
        }
    }

}

