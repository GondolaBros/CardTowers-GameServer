using CardTowers_GameServer.Shine.Messages.Interfaces;
using CardTowers_GameServer.Shine.State;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;


namespace CardTowers_GameServer.Shine.State
{
    public class ComponentStateHandler
    {
        public readonly Dictionary<string, IComponentState> StateComponents = new Dictionary<string, IComponentState>();
        private IGameMessageSerializer gameMessageSerializer;

        public ComponentStateHandler(IGameMessageSerializer gameMessageSerializer)
        {
            this.gameMessageSerializer = gameMessageSerializer;
        }

        public void AddStateComponent(IComponentState stateComponent, string componentId, string gameSessionId)
        {
            if (StateComponents.ContainsKey(componentId))
            {
                throw new InvalidOperationException($"A component with ID {componentId} has already been added.");
            }
            StateComponents[componentId] = stateComponent;
            stateComponent.ComponentId = componentId;
            stateComponent.GameSessionId = gameSessionId;
        }

        public void Update(long deltaTime)
        {
            foreach (var pair in StateComponents)
            {
                pair.Value.ProcessUpdate(deltaTime);
            }
        }

        public void SendDeltas(NetPeer clientPeer)
        {
            NetDataWriter writer = new NetDataWriter();

            foreach (var pair in StateComponents)
            {
                IComponentState stateComponent = pair.Value;
                IGameMessage? currentDelta = stateComponent.GetCurrentDelta();

                if (currentDelta != null)
                {
                    gameMessageSerializer.Serialize(currentDelta, writer);
                }
            }

            clientPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        }


        public void ReceiveAndApplyDelta(IGameMessage delta)
        {
            string componentId = delta.ComponentId;
            if (StateComponents.TryGetValue(componentId, out var component))
            {
                component.ApplyDelta(delta);
            }
            else
            {
                throw new Exception($"Component not found with id: {componentId}");
            }
        }
    }
}