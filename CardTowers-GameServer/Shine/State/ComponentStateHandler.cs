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
            Console.WriteLine("ComponentStateHandler: AddStateComponent: " + componentId + " | " + gameSessionId);
            if (StateComponents.ContainsKey(componentId))
            {
                throw new InvalidOperationException($"A component with ID {componentId} has already been added.");
            }
            StateComponents[componentId] = stateComponent;
            stateComponent.ComponentId = componentId;
            stateComponent.GameSessionId = gameSessionId;
        }

        public int Count()
        {
            return this.StateComponents.Count;
        }

        public void Update(long deltaTime)
        {
            foreach (var pair in StateComponents)
            {
                pair.Value.ProcessUpdate(deltaTime);
            }
        }


        public void SendServerActions(NetPeer clientPeer)
        {
            NetDataWriter writer = new NetDataWriter();

            foreach (var pair in StateComponents)
            {
                IComponentState stateComponent = pair.Value;
                IGameMessage serverAction = stateComponent.GetCurrentServerAction();

                if (serverAction != null)
                {
                    gameMessageSerializer.Serialize(serverAction, writer);
                    stateComponent.ResetCurrentServerAction(); // Reset the current server action after sending
                }
            }
            clientPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        }


        public void ReceiveAndApplyClientAction(IGameMessage clientAction)
        {
            string componentId = clientAction.ComponentId;
            if (StateComponents.TryGetValue(componentId, out var component))
            {
                if (component.IsValidClientAction(clientAction))
                {
                    component.ApplyClientAction(clientAction);
                }
                else
                {
                    component.HandleInvalidClientAction(clientAction);
                }
            }
            else
            {
                throw new Exception($"Component not found with id: {componentId}");
            }
        }
    }
}