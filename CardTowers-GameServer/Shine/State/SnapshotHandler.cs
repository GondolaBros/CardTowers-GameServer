using System;
using System.Collections.Generic;
using CardTowers_GameServer.Shine.Messages.Interfaces;
using CardTowers_GameServer.Shine.State;

public class SnapshotHandler
{
    private Dictionary<SnapshotKey, StateSnapshot<IGameMessage>> snapshots = new Dictionary<SnapshotKey, StateSnapshot<IGameMessage>>();


    public void StoreSnapshot(SnapshotKey snapshotKey, StateSnapshot<IGameMessage> snapshot)
    {
        snapshots[snapshotKey] = snapshot;
    }


    public StateSnapshot<IGameMessage> GetLatestSnapshot(SnapshotKey snapshotKey)
    {
        if (snapshots.TryGetValue(snapshotKey, out var snapshot))
        {
            return snapshot;
        }
        else
        {
            throw new InvalidOperationException("No snapshot exists for SnapshotKey " + snapshotKey.ToString());
        }
    }


    public StateSnapshot<IGameMessage>? GetSnapshot(SnapshotKey snapshotKey)
    {
        if (snapshots.TryGetValue(snapshotKey, out var snapshot))
        {
            return snapshot;
        }
        else
        {
            return null;
        }
    }
}

