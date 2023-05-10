using System.Diagnostics;
using CardTowers_GameServer.Shine.Data;
using CardTowers_GameServer.Shine.Entities;
using CardTowers_GameServer.Shine.Matchmaking;
using CardTowers_GameServer.Shine.Util;

namespace CardTowers_GameServer.Shine.State
{
    public class GameSession
    {
        public string Id { get; private set; }

        public Stopwatch ServerStopwatch { get; private set; }

        public List<Player> PlayerSessions { get; private set; }

        public event Action<GameSession> OnGameSessionStopped;

        public int WinnerId { get; private set; }

        public GameSession(MatchmakingEntry e1, MatchmakingEntry e2)
        {
            Id = Guid.NewGuid().ToString();

            PlayerSessions = new List<Player>(Constants.MAX_PLAYERS_STANDARD_MULTIPLAYER);

            PlayerData p1Data = new PlayerData();
            p1Data.Username = e1.Parameters.Username;
            Player p1 = new Player(e1.Connection, p1Data);

            PlayerData p2Data = new PlayerData();
            p2Data.Username = e2.Parameters.Username;
            Player p2 = new Player(e2.Connection, p2Data);

            PlayerSessions.Add(p1);
            PlayerSessions.Add(p2);

            ServerStopwatch = new Stopwatch();
        }


        public long GetElapsedTime()
        {
            return ServerStopwatch.Elapsed.Ticks;
        }


        public void Start()
        {
            Console.WriteLine("Started game session: " + Id
                + " | Players: " + PlayerSessions[0].Data.Username + " | " + PlayerSessions[1].Data.Username);

            ServerStopwatch = Stopwatch.StartNew();
        }


        public void Cleanup()
        {
            PlayerSessions.Clear();
        }


        public void Stop(Player winner)
        {
            ServerStopwatch.Stop();
            WinnerId = winner.Connection.Id;

            Console.WriteLine("Game session stopped: " + Id);
            Console.WriteLine(winner.Data.Username + " is the winner!");
            Console.WriteLine("Cleaning up game session, send final state to clients.");

            OnGameSessionStopped(this);
        }


        public bool HasPlayer(int id)
        {
            return PlayerSessions.Exists(p => p.Connection.Id == id);
        }


        public void PlayerDisconnected(Player player)
        {

            Console.WriteLine("GameSession: PlayerDisconnected: " + player.Data.Username);

            // Handle player disconnection, possibly end the game and declare the other player as the winner.
            // Remove the player from the session
            PlayerSessions.Remove(player);

            // If there's only one player left, they are the winner
            if (PlayerSessions.Count == 1)
            {
                Console.WriteLine("GameSession player count: " + PlayerSessions.Count);
                Stop(PlayerSessions[0]);
            }
        }
    }
}

