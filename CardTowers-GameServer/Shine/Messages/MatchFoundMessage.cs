using System;
using CardTowers_GameServer.Shine.Messages;
using LiteNetLib;
using LiteNetLib.Utils;

public class MatchFoundMessage : IHandledMessage
{
    // details needed for match

	public MatchFoundMessage()
	{
	}

    public void Deserialize(NetDataReader reader)
    {
        //throw new NotImplementedException();
    }

    public void Serialize(NetDataWriter writer)
    {
        // throw new NotImplementedException();
    }


    public void Handle(NetPeer peer)
    {
        //throw new NotImplementedException();
        // handle on client
    }
}

