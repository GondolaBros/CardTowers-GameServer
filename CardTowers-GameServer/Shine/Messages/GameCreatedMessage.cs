using CardTowers_GameServer.Shine.Messages;
using LiteNetLib;
using LiteNetLib.Utils;

public class GameCreatedMessage : IHandledMessage
{
    public string Id { get; set; }
    public long ElapsedTicks { get; set; }


    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetString();
        ElapsedTicks = reader.GetLong();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(ElapsedTicks);
    }

    public void Handle(NetPeer peer)
    {

    }
}

