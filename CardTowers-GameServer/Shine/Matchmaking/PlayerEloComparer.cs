using System;
namespace CardTowers_GameServer.Shine.Matchmaking
{
    public class PlayerEloComparer : IComparer<MatchmakingEntry>
    {
        public int Compare(MatchmakingEntry? x, MatchmakingEntry? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.Parameters.EloRating.CompareTo(y.Parameters.EloRating);
        }
    }
}

