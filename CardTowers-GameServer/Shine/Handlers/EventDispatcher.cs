using System;
using System.Collections.Generic;
using LiteNetLib;
using CardTowers_GameServer.Shine.Messages;
using CardTowers_GameServer.Shine.Messages.Interfaces;

namespace CardTowers_GameServer.Shine
{
    public class EventDispatcher
    {
        private Dictionary<byte, Action<INetworkMessage, NetPeer>> messageHandlers;

        public EventDispatcher()
        {
            messageHandlers = new Dictionary<byte, Action<INetworkMessage, NetPeer>>();
        }

        public void RegisterHandler<T>(Action<T, NetPeer> handler, byte channel) where T : INetworkMessage
        {
            messageHandlers[channel] = (message, peer) => handler((T)message, peer);
        }

        public void DispatchMessage(INetworkMessage message, NetPeer peer, byte channel)
        {
            if (messageHandlers.ContainsKey(channel))
            {
                messageHandlers[channel]?.Invoke(message, peer);
            }
        }
    }
}
