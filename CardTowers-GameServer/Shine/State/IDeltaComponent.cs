using System;
namespace CardTowers_GameServer.Shine.State
{
    public interface IDeltaComponent
    {
        IDelta GenerateDelta();
        IDelta CreateNewDeltaInstance();
        void ApplyDelta(IDelta delta);
        void BaseUpdate(long deltaTime);
    }
}

