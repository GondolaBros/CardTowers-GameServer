using System;
namespace CardTowers_GameServer.Shine.State
{
    public struct SnapshotKey
    {
        public string ComponentId { get; }
        public GameMessageType ComponentType { get; }

        public SnapshotKey(string componentId, GameMessageType componentType)
        {
            ComponentId = componentId;
            ComponentType = componentType;
        }
    }
}

