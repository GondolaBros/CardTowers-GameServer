namespace CardTowers_GameServer.Shine.State
{
    public interface IGameStateComponent<TDelta> : IDeltaComponent where TDelta : IDelta
    {
        Frequency Frequency { get; }
        void Update(long deltaTime);
        TDelta GenerateDelta();
        void ApplyDelta(TDelta delta);
    }
}

