using System;
using System.Collections.Generic;
using LiteNetLib;
using CardTowers_GameServer.Shine.Messages.Interfaces;

namespace CardTowers_GameServer.Shine
{
    public class EventDispatcher
    {
        private Dictionary<byte, Action<ISystemMessage, NetPeer>> systemMessageHandlers;
        private Dictionary<byte, Action<IGameMessage, NetPeer>> gameMessageHandlers;

        public EventDispatcher()
        {
            systemMessageHandlers = new Dictionary<byte, Action<ISystemMessage, NetPeer>>();
            gameMessageHandlers = new Dictionary<byte, Action<IGameMessage, NetPeer>>();
        }


        public void RegisterSystemMessageHandler<T>(Action<T, NetPeer> handler, byte channel) where T : ISystemMessage
        {
            systemMessageHandlers[channel] = (message, peer) => handler((T)message, peer);
        }


        public void RegisterGameMessageHandler<T>(Action<T, NetPeer> handler, byte channel) where T : IGameMessage
        {
            gameMessageHandlers[channel] = (message, peer) => handler((T)message, peer);
        }


        public void DispatchSystemMessage(ISystemMessage message, NetPeer peer, byte channel)
        {
            if (systemMessageHandlers.ContainsKey(channel))
            {
                systemMessageHandlers[channel]?.Invoke(message, peer);
            }
        }

        public void DispatchGameMessage(IGameMessage message, NetPeer peer, byte channel)
        {
            if (gameMessageHandlers.ContainsKey(channel))
            {
                gameMessageHandlers[channel]?.Invoke(message, peer);
            }
        }
    }
}
