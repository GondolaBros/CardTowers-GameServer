using System;
using LiteNetLib;
using LiteNetLib.Utils;
using CardTowers_GameServer.Shine;
using CardTowers_GameServer.Shine.Interfaces;
using CardTowers_GameServer.Shine.Handlers;
using CardTowers_GameServer.Shine.Data;
using CardTowers_GameServer.Shine.Models;

public class MatchmakingMessage : IHandledMessage
{
    public string Username { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        Username = reader.GetString();
    }


    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Username);
    }


    public void Handle(NetPeer peer)
    {
        Console.WriteLine("Incoming MatchmakingMessage: " + Username + " | ID: " + peer.Id);
        Connection? connectedPeer = ServerHandler.GetPeerById(peer.Id);

        if (connectedPeer == null)
        {
            throw new Exception("Unhandled matchmaking request");
        }

       
        MatchmakingParameters matchmakingParameters = new MatchmakingParameters();
        matchmakingParameters.EloRating = 1500;
        MatchmakingEntry matchmakingEntry =
            new MatchmakingEntry(connectedPeer, matchmakingParameters);

        MatchmakingHandler.Instance.Enqueue(matchmakingEntry);
    }
}

