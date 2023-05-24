using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

namespace CardTowers_GameServer.Shine.State
{
    public class GameStateHandler
    {
        private readonly Dictionary<DeltaType, IDeltaComponent> stateComponents = new();

        private SnapshotHandler snapshotManager = new();


        private NetPacketProcessor netPacketProcessor;

        public GameStateHandler(NetPacketProcessor netPacketProcessor)
        {
            this.netPacketProcessor = netPacketProcessor;
        }

       
        public void AddStateComponent<TDelta>(GameStateComponentBase<TDelta> stateComponent) where TDelta : IDelta, new()
        {
            DeltaType deltaType = stateComponent.GenerateDelta().Type;
            if (stateComponents.ContainsKey(deltaType))
            {
                throw new InvalidOperationException("A component with DeltaType " + deltaType.ToString() + " has already been added.");
            }
            stateComponents[deltaType] = stateComponent;
        }


        public void GenerateAndSendAllDeltas(NetPeer clientPeer)
        {
            NetDataWriter writer = new NetDataWriter();

            foreach (var pair in stateComponents)
            {
                if (pair.Value.Frequency != Frequency.EventBased)
                {
                    IDelta delta = pair.Value.GenerateDelta();
                    if (delta != null)
                    {
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
                pair.Value.BaseUpdate(deltaTime);
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


        public void CheckComponentConsistency(DeltaType type)
        {
            if (stateComponents.TryGetValue(type, out var component))
            {
                if (!component.IsStateConsistent())
                {
                    ApplySnapshot(type);
                }
            }
        }


        public void GenerateAndStoreAllSnapshots()
        {
            var newSnapshots = new Dictionary<DeltaType, GameStateSnapshot<IDelta>>();
            foreach (var pair in stateComponents)
            {
                var snapshot = pair.Value.GenerateSnapshot();
                newSnapshots[pair.Key] = snapshot;
            }
            snapshotManager.StoreSnapshot(newSnapshots);
        }


        public void ApplySnapshot(DeltaType type)
        {
            GameStateSnapshot<IDelta> snapshot = snapshotManager.GetLatestSnapshot(type);

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
}

