using System;
using CardTowers_GameServer.Shine.Messages.Interfaces;

namespace CardTowers_GameServer.Shine.State
{
    public class StateSnapshot<TGameMessage> where TGameMessage : IGameMessage
    {
        public TGameMessage State { get; private set; }
        public long TimeStamp { get; private set; }  // Time when the snapshot was taken

        public StateSnapshot(TGameMessage state, long timeStamp)
        {
            State = state;
            TimeStamp = timeStamp;
        }
    }
}