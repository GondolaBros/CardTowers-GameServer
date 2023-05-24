using System;
namespace CardTowers_GameServer.Shine.State
{
    public interface IDeltaComponent
    {
        Frequency Frequency { get; }

        IDelta GenerateDelta();
        IDelta CreateNewDeltaInstance();
        void ApplyDelta(IDelta delta);
        void BaseUpdate(long deltaTime);
        bool IsStateConsistent();
        GameStateSnapshot<IDelta> GenerateSnapshot();
    }
}

