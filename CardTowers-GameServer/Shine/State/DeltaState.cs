using System;
namespace CardTowers_GameServer.Shine.State
{
    public abstract class DeltaState
    {
        public abstract void Apply(IGameState state);
    }

}

