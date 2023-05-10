using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CardTowers_GameServer.Shine.Matchmaking;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class MatchmakingHandler
    {
        private static MatchmakingHandler? _instance;
        private ServerHandler? _serverHandler;
        private const int MaxEloDifference = 100;

        private List<MatchmakingEntry> _queue;
        private readonly object _queueLock = new object();

        public EventHandler<MatchFoundEventArgs>? OnMatchFound;

        private MatchmakingHandler()
        {
            _queue = new List<MatchmakingEntry>();
        }

        public static MatchmakingHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MatchmakingHandler();
                }
                return _instance;
            }
        }


        public void SetServerHandler(ServerHandler serverHandler)
        {
            _serverHandler = serverHandler;
        }


        public int Count()
        {
            lock (_queueLock)
            {
                return _queue.Count;
            }
        }


        public void Enqueue(MatchmakingEntry player)
        {
            lock (_queueLock)
            {
                int index = _queue.BinarySearch(player, new PlayerEloComparer());
                if (index < 0)
                {
                    index = ~index;
                }
                _queue.Insert(index, player);

                Console.WriteLine("Added player entry to matchmaking queue: " + player.Parameters.Username + " | Count: " + _queue.Count);
            }
        }


        public bool TryRemove(MatchmakingEntry player)
        {
            //Console.WriteLine("TryRemove player from queue: " + player.Parameters.Username + " | Count: " + _queue.Count);

            lock (_queueLock)
            {
                bool removed = _queue.Remove(player);
                Console.WriteLine("Removed player entry from matchmaking queue: " + player.Parameters.Username + ": " + removed);
                return removed;
            }
        }


        public MatchmakingEntry? GetPlayerById(int id)
        {
            lock (_queueLock)
            {
                return _queue.Find(p => p.Connection.Id == id);
            }
        }


        public MatchmakingEntry? FindMatch(MatchmakingEntry player, int maxEloDifference)
        {
            int index = _queue.BinarySearch(player, new PlayerEloComparer());
            if (index < 0)
            {
                index = ~index;
            }

            MatchmakingEntry? bestMatch = null;
            int minEloDifference = int.MaxValue;

            for (int i = index - 1; i >= 0 && player.Parameters.EloRating - _queue[i].Parameters.EloRating <= maxEloDifference; i--)
            {
                if (_queue[i].Connection.Id == player.Connection.Id)
                {
                    continue;
                }

                int eloDifference = player.Parameters.EloRating - _queue[i].Parameters.EloRating;
                if (eloDifference < minEloDifference)
                {
                    bestMatch = _queue[i];
                    minEloDifference = eloDifference;
                }
            }

            for (int i = index; i < _queue.Count && _queue[i].Parameters.EloRating - player.Parameters.EloRating <= maxEloDifference; i++)
            {
                if (_queue[i].Connection.Id == player.Connection.Id)
                {
                    continue;
                }

                int eloDifference = _queue[i].Parameters.EloRating - player.Parameters.EloRating;
                if (eloDifference < minEloDifference)
                {
                    bestMatch = _queue[i];
                    minEloDifference = eloDifference;
                }
            }

            return bestMatch;
        }


        public async Task PeriodicMatchmakingAsync(int searchDelayMilliseconds = 500)
        {
            while (true)
            {
                //Console.WriteLine("PeriodicMatchmaking: queue size: " + _queue.Count);
                List<MatchmakingEntry> playersSnapshot;

                lock (_queueLock)
                {
                    playersSnapshot = new List<MatchmakingEntry>(_queue);
                }

                foreach (var player in playersSnapshot)
                {
                    //Console.WriteLine("Checking player: " + player.Username);

                    MatchmakingEntry? opponent = FindMatch(player, MaxEloDifference);

                    if (opponent != null)
                    {
                        MatchFoundEventArgs args = new MatchFoundEventArgs(player, opponent);
                        OnMatchFound?.Invoke(this, args);

                        // Remove both players from the queue.
                        if (TryRemove(player) && TryRemove(opponent))
                        {
                            //Console.WriteLine("Match created for: " + player.Data.Username + " | " + opponent.Data.Username);
                            Console.WriteLine("PeriodicMatchmakingAsync | Match found, removed both player entries from queue!");

                        }
                    }
                    else
                    {
                        //Console.WriteLine("PeriodicMatchmaking: no match found for: " + player.Username);
                    }
                }

                await Task.Delay(searchDelayMilliseconds);
            }
        }
    }


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
