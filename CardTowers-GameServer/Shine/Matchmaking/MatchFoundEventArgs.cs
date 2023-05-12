namespace CardTowers_GameServer.Shine.Matchmaking
{
    public class MatchFoundEventArgs : EventArgs
    {
        public MatchmakingEntry P1 { get; private set; }
        public MatchmakingEntry P2 { get; private set; }

        // do this if we wana scale game further
        // public List<MatchmakingEntry {get; private set; }

        public MatchFoundEventArgs(MatchmakingEntry p1, MatchmakingEntry p2)
        {
            P1 = p1;
            P2 = p2;
        }
    }
}