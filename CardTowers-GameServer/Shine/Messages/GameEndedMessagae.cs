using LiteNetLib.Utils;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardTowers_GameServer.Shine.Messages;
using CardTowers_GameServer.Shine.Messages.Interfaces;

public class GameEndedMessagae : INetworkMessage
{
    public long ElapsedTicks { get; set; }
    public int WinnerId { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        ElapsedTicks = reader.GetLong();
        WinnerId = reader.GetInt();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ElapsedTicks);
        writer.Put(WinnerId);
    }


    public void Handle(NetPeer peer)
    {

    }
}
