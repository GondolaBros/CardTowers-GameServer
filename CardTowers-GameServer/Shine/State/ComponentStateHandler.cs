using CardTowers_GameServer.Shine.State;
using LiteNetLib;
using LiteNetLib.Utils;

public class ComponentStateHandler
{
    private readonly Dictionary<DeltaType, IComponentState<IDelta>> stateComponents = new Dictionary<DeltaType, IComponentState<IDelta>>();
    private NetPacketProcessor netPacketProcessor;

    public ComponentStateHandler(NetPacketProcessor netPacketProcessor)
    {
        this.netPacketProcessor = netPacketProcessor;
    }

    public void AddStateComponent<TDelta>(IComponentState<TDelta> stateComponent) where TDelta : IDelta, new()
    {
        DeltaType deltaType = stateComponent.GenerateDelta().Type;
        if (stateComponents.ContainsKey(deltaType))
        {
            throw new InvalidOperationException("A component with DeltaType " + deltaType.ToString() + " has already been added.");
        }
        stateComponents[deltaType] = (IComponentState<IDelta>)stateComponent;
    }

    public void GenerateAndSendAllDeltas(NetPeer clientPeer, SnapshotHandler snapshotHandler, long deltaTime)
    {
        NetDataWriter writer = new NetDataWriter();

        foreach (var pair in stateComponents)
        {
            if (pair.Value.Frequency != Frequency.EventBased)
            {
                IDelta delta = pair.Value.GenerateDelta();
                if (delta != null)
                {
                    // Apply the generated delta to the component
                    pair.Value.ApplyDelta(delta);

                    // Generate a snapshot key
                    SnapshotKey snapshotKey = new SnapshotKey(Guid.NewGuid(), delta.Type);

                    // Generate a new snapshot
                    StateSnapshot<IDelta> newSnapshot = new StateSnapshot<IDelta>(delta, deltaTime);

                    // Store the new snapshot
                    snapshotHandler.StoreSnapshot(snapshotKey, newSnapshot);

                    // Write the delta type to the packet
                    writer.Put((int)delta.Type);

                    // Serialize the delta
                    delta.Serialize(writer);
                }
            }
        }

        // Send the packet containing all deltas to the client
        clientPeer.Send(writer, DeliveryMethod.ReliableOrdered);
    }



    public void ReceiveAndApplyDelta(NetPacketReader packet, NetPeer sender)
    {
        while (packet.AvailableBytes > 0)
        {
            // Read the delta type from the packet
            DeltaType deltaType = (DeltaType)packet.GetInt();

            if (stateComponents.TryGetValue(deltaType, out var component))
            {
                // Create a new instance of the delta
                IDelta delta = component.CreateNewDeltaInstance();

                // Deserialize the delta from the packet
                delta.Deserialize(packet);

                // Apply the delta to the component
                component.ApplyDelta(delta);
            }
            else
            {
                throw new InvalidOperationException("Delta type " + deltaType.ToString() + " has not been registered.");
            }
        }
    }

    public void UpdateAll(long deltaTime)
    {
        foreach (var pair in stateComponents)
        {
            pair.Value.InternalUpdate(deltaTime);
        }
    }

    public Dictionary<DeltaType, IDelta> GenerateAllDeltas()
    {
        var deltas = new Dictionary<DeltaType, IDelta>();
        foreach (var pair in stateComponents)
        {
            var delta = pair.Value.GenerateDelta();
            if (delta != null)
            {
                deltas[pair.Key] = delta;
            }
        }
        return deltas;
    }

    public void ApplySnapshot(DeltaType type, Guid entityId, SnapshotHandler snapshotHandler)
    {
        SnapshotKey snapshotKey = new SnapshotKey(entityId, type);
        StateSnapshot<IDelta> snapshot = snapshotHandler.GetLatestSnapshot(snapshotKey);

        if (stateComponents.TryGetValue(type, out var component))
        {
            component.ApplyDelta(snapshot.State);
        }
        else
        {
            throw new InvalidOperationException("Delta type " + type.ToString() + " has not been registered.");
        }
    }
}
