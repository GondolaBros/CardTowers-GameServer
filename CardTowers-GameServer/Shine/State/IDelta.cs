using LiteNetLib.Utils;

namespace CardTowers_GameServer.Shine.State
{
    public interface IDelta : INetSerializable
    {
        DeltaType Type { get; }
    }
}
