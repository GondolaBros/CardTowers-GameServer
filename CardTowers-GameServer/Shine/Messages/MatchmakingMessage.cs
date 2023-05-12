using LiteNetLib;
using LiteNetLib.Utils;
using CardTowers_GameServer.Shine.Interfaces;
using CardTowers_GameServer.Shine.Handlers;
using CardTowers_GameServer.Shine.Matchmaking;
using CardTowers_GameServer.Shine.Util;

public class MatchmakingMessage : IHandledMessage
{
    //public string Username { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        //Username = reader.GetString();
    }


    public void Serialize(NetDataWriter writer)
    {
        //writer.Put(Username);
    }


    public void Handle(NetPeer peer)
    {
        //Console.WriteLine("Incoming MatchmakingMessage: " + Username + " | ID: " + peer.Id);

        NetEvents.InvokeMatchmakingEntryReceived(peer);
    }
}

