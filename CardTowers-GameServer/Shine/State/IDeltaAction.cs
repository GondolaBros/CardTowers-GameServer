using System;

namespace CardTowers_GameServer.Shine.State
{
    public interface IDeltaAction<TDelta> where TDelta : Delta
    {
        void Execute(IDeltaObject<TDelta> deltaObject);
    }
}
