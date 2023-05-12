using LiteNetLib;

namespace CardTowers_GameServer.Shine.Util
{
    public class MatchmakingEntryReceivedEventArgs : EventArgs
    {
        public NetPeer Peer { get; private set; }

        public MatchmakingEntryReceivedEventArgs(NetPeer peer)
        {
            this.Peer = peer;
        }
    }
}