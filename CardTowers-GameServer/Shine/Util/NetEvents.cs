using LiteNetLib;

namespace CardTowers_GameServer.Shine.Util
{
	public class NetEvents
	{
        public static event Action<MatchmakingEntryReceivedEventArgs>? OnMatchmakingEntryReceived;

		public static void InvokeMatchmakingEntryReceived(NetPeer peer)
		{
			MatchmakingEntryReceivedEventArgs args = new MatchmakingEntryReceivedEventArgs(peer);
			OnMatchmakingEntryReceived?.Invoke(args);
		}
	}
}

