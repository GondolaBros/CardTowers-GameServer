using System;
namespace CardTowers_GameServer.Shine.State
{
    public class GameStateSnapshot<TDelta> where TDelta : IDelta
    {
        public TDelta State { get; private set; }
        public long Timestamp { get; private set; }

        public GameStateSnapshot(TDelta state)
        {
            State = state;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}

