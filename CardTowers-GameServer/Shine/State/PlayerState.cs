﻿using CardTowers_GameServer.Shine.Models;

namespace CardTowers_GameServer.Shine.State
{
    public class PlayerState
    {
        //public Dictionary<string, IStateComponent> StateComponents { get; private set; }

        public PlayerState()
        {
          // StateComponents = new Dictionary<string, IStateComponent>();
        }

        public void Update(long deltaTime)
        {
            /*
            foreach (var component in StateComponents.Values)
            {
                component.Update(deltaTime);
            }*/
        }
    }
}

