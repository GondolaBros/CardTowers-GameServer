using System;
namespace CardTowers_GameServer.Shine.State
{
    public class SnapshotHandler
    {
        private Dictionary<DeltaType, Queue<GameStateSnapshot<IDelta>>> snapshots = new();
        private const int snapshotHistorySize = 100; // store the last 100 snapshots

        public void StoreSnapshot(Dictionary<DeltaType, GameStateSnapshot<IDelta>> newSnapshots)
        {
            foreach (var pair in newSnapshots)
            {
                if (!snapshots.ContainsKey(pair.Key))
                {
                    snapshots[pair.Key] = new Queue<GameStateSnapshot<IDelta>>();
                }

                if (snapshots[pair.Key].Count >= snapshotHistorySize)
                {
                    snapshots[pair.Key].Dequeue();
                }

                snapshots[pair.Key].Enqueue(pair.Value);
            }
        }

        public GameStateSnapshot<IDelta> GetLatestSnapshot(DeltaType type)
        {
            if (!snapshots.ContainsKey(type))
                throw new Exception("No snapshots for DeltaType " + type.ToString());

            return snapshots[type].Peek();
        }
    }
}

