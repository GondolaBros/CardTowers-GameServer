namespace CardTowers_GameServer.Shine.State
{
    public interface IComponentState<TDelta> where TDelta : IDelta
    {
        Frequency Frequency { get; }
        TDelta GenerateDelta();
        void ApplyDelta(TDelta delta);
        void Update(long deltaTime);
        void InternalUpdate(long deltaTime);
        bool IsStateConsistent();
        IDelta CreateNewDeltaInstance();
    }
}

