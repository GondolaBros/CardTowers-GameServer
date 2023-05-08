using LiteNetLib;
using LiteNetLib.Utils;

using CardTowers_GameServer.Shine.Interfaces;


public class MatchFoundMessage : IHandledMessage
{
    public string OpponentUsername { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        OpponentUsername = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(OpponentUsername);
    }

    public void Handle(NetPeer peer)
    {

        // Handle logic will be implemented in the ClientHandler
    }
}

