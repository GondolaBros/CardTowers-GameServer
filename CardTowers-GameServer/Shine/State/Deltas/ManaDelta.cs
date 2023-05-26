using CardTowers_GameServer.Shine.State;
using LiteNetLib.Utils;


namespace CardTowers_GameServer.Shine.State.Deltas
{
    public class ManaDelta : IDelta
    {
        public DeltaType Type { get { return DeltaType.Mana; } }
        public int ManaChange { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ManaChange);
        }

        public void Deserialize(NetDataReader reader)
        {
            ManaChange = reader.GetInt();
        }
    }
}
