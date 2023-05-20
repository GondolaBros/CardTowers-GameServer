using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CardTowers_GameServer.Shine.Matchmaking;
using Microsoft.Extensions.Logging;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class MatchmakingHandler
    {
        private ServerHandler? _serverHandler;
        private const int MaxEloDifference = 100;

        private List<MatchmakingEntry> _queue;
        private readonly object _queueLock = new object();
        private ConcurrentDictionary<int, CancellationTokenSource> playerMatchmakingTasks = new ConcurrentDictionary<int, CancellationTokenSource>();

        public EventHandler<MatchFoundEventArgs>? OnMatchFound;

        ILogger logger;


        public MatchmakingHandler(ServerHandler serverHandler, ILogger logger)
        {
            _queue = new List<MatchmakingEntry>();
            _serverHandler = serverHandler;
            this.logger = logger;
        }



        public void Enqueue(MatchmakingEntry entry)
        {
            lock (_queueLock)
            {
                if (!playerMatchmakingTasks.ContainsKey(entry.Parameters.Id))
                {
                    int index = _queue.BinarySearch(entry, new PlayerEloComparer());
                    if (index < 0)
                    {
                        index = ~index;
                    }
                    _queue.Insert(index, entry);

                    logger.LogInformation("Added player entry to matchmaking queue: " + entry.Parameters.Username + " | Count: " + _queue.Count);

                    // Start matchmaking for this player
                    StartMatchmakingForPlayer(entry);
                }
                else
                {
                    logger.LogInformation("Player " + entry.Parameters.Username + " is already in matchmaking process");
                }
            }
        }



        private void StartMatchmakingForPlayer(MatchmakingEntry entry)
        {
            // Create a cancellation token source and add it to the dictionary
            var cts = new CancellationTokenSource();
            playerMatchmakingTasks[entry.Parameters.Id] = cts;

            // Start a task to matchmake this player
            Task.Run(() => MatchmakePlayer(entry, cts.Token));
        }


        private async Task MatchmakePlayer(MatchmakingEntry player, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var opponent = FindMatch(player, MaxEloDifference);
                if (opponent != null)
                {
                    lock (_queueLock)
                    {
                        // Ensure player and opponent aren't already matched
                        if (player.IsMatched || opponent.IsMatched)
                        {
                            return;
                        }

                        player.IsMatched = true;
                        opponent.IsMatched = true;
                    }

                    // Match found, trigger the event
                    MatchFoundEventArgs args = new MatchFoundEventArgs(player, opponent);
                    OnMatchFound?.Invoke(this, args);

                    // Remove players from the queue and stop matchmaking for them
                    if (TryRemove(player) && TryRemove(opponent))
                    {
                        StopMatchmakingForPlayer(player.Parameters.Id);
                        StopMatchmakingForPlayer(opponent.Parameters.Id);
                    }
                    return; // Match found, end this task
                }
                else
                {
                    await Task.Delay(500, ct); // No match found, wait and try again
                }
            }
        }


        public MatchmakingEntry? FindMatch(MatchmakingEntry entry, int maxEloDifference)
        {
            int index = _queue.BinarySearch(entry, new PlayerEloComparer());
            if (index < 0)
            {
                index = ~index;
            }

            MatchmakingEntry? bestMatch = null;
            int minEloDifference = int.MaxValue;

            for (int i = index - 1; i >= 0 && entry.Parameters.EloRating - _queue[i].Parameters.EloRating <= maxEloDifference; i--)
            {
                if (_queue[i].Parameters.Id == entry.Parameters.Id || _queue[i].IsMatched)
                {
                    continue;
                }

                int eloDifference = entry.Parameters.EloRating - _queue[i].Parameters.EloRating;
                if (eloDifference < minEloDifference)
                {
                    bestMatch = _queue[i];
                    minEloDifference = eloDifference;
                }
            }

            for (int i = index; i < _queue.Count && _queue[i].Parameters.EloRating - entry.Parameters.EloRating <= maxEloDifference; i++)
            {
                if (_queue[i].Parameters.Id == entry.Parameters.Id || _queue[i].IsMatched)
                {
                    continue;
                }

                int eloDifference = _queue[i].Parameters.EloRating - entry.Parameters.EloRating;
                if (eloDifference < minEloDifference)
                {
                    bestMatch = _queue[i];
                    minEloDifference = eloDifference;
                }
            }

            return bestMatch;
        }




        private void StopMatchmakingForPlayer(int playerId)
        {
            if (playerMatchmakingTasks.TryRemove(playerId, out var cts))
            {
                // If we found a matchmaking task for this player, we cancel it
                cts.Cancel();
                var entry = GetMatchmakingEntryById(playerId);
                if (entry != null)
                {
                    entry.IsMatched = false;
                }
            }
        }



        public bool TryRemove(MatchmakingEntry entry)
        {
            lock (_queueLock)
            {
                bool removed = _queue.Remove(entry);
                logger.LogInformation("Removed player entry from matchmaking queue: " + entry.Parameters.Username + ": " + removed);
                if (removed)
                {
                    // If the player was successfully removed from the queue, we also stop his matchmaking task
                    entry.IsMatched = false;
                    StopMatchmakingForPlayer(entry.Parameters.Id);
                }
                return removed;
            }
        }



        public MatchmakingEntry? GetMatchmakingEntryById(int id)
        {
            lock (_queueLock)
            {
                return _queue.Find(p => p.Parameters.Id == id);
            }
        }


        public int Count()
        {
            lock (_queueLock)
            {
                return _queue.Count;
            }
        }
    }
}

