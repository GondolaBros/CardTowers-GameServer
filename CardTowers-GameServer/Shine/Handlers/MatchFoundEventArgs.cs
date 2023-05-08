using CardTowers_GameServer.Shine.Data;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class MatchFoundEventArgs : EventArgs
    {
        public MatchmakingEntry Player1 { get; private set; }
        public MatchmakingEntry Player2 { get; private set; }

        // do this if we wana scale game further
        // public List<MatchmakingEntry {get; private set; }

        public MatchFoundEventArgs(MatchmakingEntry p1, MatchmakingEntry p2)
        {
            this.Player1 = p1;
            this.Player2 = p2;
        }
    }
}