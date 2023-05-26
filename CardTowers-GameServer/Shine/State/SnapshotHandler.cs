using System;
namespace CardTowers_GameServer.Shine.State
{
    public class SnapshotHandler
    {
        private Dictionary<SnapshotKey, Queue<StateSnapshot<IDelta>>> snapshots = new();
        private const int snapshotHistorySize = 100; // store the last 100 snapshots

        public void StoreSnapshot<TDelta>(SnapshotKey key, StateSnapshot<TDelta> newSnapshot) where TDelta : IDelta
        {
            if (!snapshots.ContainsKey(key))
            {
                snapshots[key] = new Queue<StateSnapshot<IDelta>>();
            }

            if (snapshots[key].Count >= snapshotHistorySize)
            {
                snapshots[key].Dequeue();
            }

            snapshots[key].Enqueue(newSnapshot as StateSnapshot<IDelta>);
        }

        public StateSnapshot<IDelta> GetLatestSnapshot(SnapshotKey key)
        {
            if (!snapshots.ContainsKey(key))
                throw new Exception("No snapshots for key: " + key.EntityId + " - " + key.ComponentType);

            return snapshots[key].Peek();
        }
    }
}

