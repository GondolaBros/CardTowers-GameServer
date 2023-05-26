using System;
namespace CardTowers_GameServer.Shine.State
{
    public class StateSnapshot<TDelta> where TDelta : IDelta
    {
        public TDelta State { get; private set; }
        public long Timestamp { get; private set; }
        public long GameTime { get; private set; }

        public StateSnapshot(TDelta state, long gameTime)
        {
            State = state;
            GameTime = gameTime;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}