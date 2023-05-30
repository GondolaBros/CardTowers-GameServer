using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State.Components;

namespace CardTowers_GameServer.Shine.State
{
    public class PlayerState
    {
        public ComponentStateHandler ComponentStateHandler;
        public IComponentState ManaComponent { get; private set; }
        public IGameMessageSerializer GameMessageSerializer { get; private set; }

        public PlayerState(string gameSessionId)
        {
            GameMessageSerializer = new GameMessageSerializer();
            ComponentStateHandler = new ComponentStateHandler(GameMessageSerializer);

            ManaComponent = new ManaComponent(Frequency.Moderate);

            ComponentStateHandler.AddStateComponent(ManaComponent, Guid.NewGuid().ToString(), gameSessionId);
            Console.WriteLine("PlayerState - AddedStateComponent | Total Components: " + ComponentStateHandler.Count());
        }


        public IComponentState GetManaComponent()
        {
            return ManaComponent;
        }
    }
}

