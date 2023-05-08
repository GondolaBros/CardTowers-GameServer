using LiteNetLib;
using System;

namespace CardTowers_GameServer.Shine.Data
{
	public class Connection
	{
		public int Id { get; private set; }
		public NetPeer Peer { get; private set; }
		public DateTime ConnectionTime { get; private set; }

        public Connection(NetPeer peer)
		{
			this.Id = peer.Id;
			this.Peer = peer;
			ConnectionTime = DateTime.UtcNow;
		}
	}
}

