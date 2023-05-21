using System;

namespace CardTowers_GameServer.Shine.State
{
    public interface IDeltaObject<TDelta> where TDelta : Delta
    {
        TDelta GenerateDelta();
        void ApplyDelta(TDelta delta);
    }

}

