using System;
namespace CardTowers_GameServer.Shine.State
{
    public struct SnapshotKey
    {
        public Guid EntityId { get; }
        public DeltaType ComponentType { get; }

        public SnapshotKey(Guid entityId, DeltaType componentType)
        {
            EntityId = entityId;
            ComponentType = componentType;
        }
    }
}

