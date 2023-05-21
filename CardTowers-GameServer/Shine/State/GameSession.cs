using System;
using System.Collections.Generic;
using System.Diagnostics;
using CardTowers_GameServer.Shine.Data;
using CardTowers_GameServer.Shine.Matchmaking;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.Util;

namespace CardTowers_GameServer.Shine.State
{
    public class GameSession
    {
        public string Id { get; private set; }

        public Stopwatch ServerStopwatch { get; private set; }
        public List<Player> PlayerSessions { get; private set; }
        public event Action<GameSession> OnGameSessionStopped;

        //public PlayerState Player1State { get; private set; }
        //public PlayerState Player2State { get; private set; }

        private long lastTickTime;
        private long accumulatedDeltaTime;

        public int WinnerId { get; private set; }

        public GameSession(Player p1, Player p2)
        {
            ServerStopwatch = Stopwatch.StartNew();
            lastTickTime = ServerStopwatch.ElapsedMilliseconds;

            Id = Guid.NewGuid().ToString();

            PlayerSessions = new List<Player>(Constants.MAX_PLAYERS_STANDARD_MULTIPLAYER);
            PlayerSessions.Add(p1);
            PlayerSessions.Add(p2);

            //Player1State = new PlayerState(p1, new GameMap());
            //Player2State = new PlayerState(p2, new GameMap());

            accumulatedDeltaTime = 0;
        }

        public void Update()
        {
            // Calculate delta time
            long currentTickTime = ServerStopwatch.ElapsedMilliseconds;
            long deltaTime = currentTickTime - lastTickTime;
            lastTickTime = currentTickTime;

            // Accumulate delta time
            accumulatedDeltaTime += deltaTime;

            /*
            // If enough time has passed, increase Elixir
            if (accumulatedDeltaTime >= Constants.ELIXIR_GENERATION_INTERVAL_MS)
            {
                var elixirAction = new SpendElixirAction(-Constants.ELIXIR_PER_INTERVAL);
                Player1State.ApplyDeltaAction(elixirAction);
                Player2State.ApplyDeltaAction(elixirAction);

                accumulatedDeltaTime -= Constants.ELIXIR_GENERATION_INTERVAL_MS;
            }*/
        }


        /*
        public void UpdateGameState(Player player, IDeltaAction deltaAction)
        {
            // Get the appropriate PlayerState instance based on the player
            PlayerState playerState = player == PlayerSessions[0] ? Player1State : Player2State;

            // Apply delta action to the player state
            try
            {
                playerState.ApplyDeltaAction(deltaAction);
            }
            catch (Exception e)
            {
                // Handle the exception (for example, if the player doesn't have enough elixir to play the card)
                Console.WriteLine($"Error applying delta action: {e.Message}");
            }
        }*/

        public long GetElapsedTime()
        {
            return ServerStopwatch.Elapsed.Ticks;
        }

        public void Cleanup()
        {
            PlayerSessions.Clear();
        }

        public void Stop(Player winner)
        {
            ServerStopwatch.Stop();
            WinnerId = winner.Peer.Id;

            Console.WriteLine("Game session stopped: " + Id);
            Console.WriteLine(winner.Entity.display_name + " is the winner!");
            Console.WriteLine("Cleaning up game session, send final state to clients.");

            OnGameSessionStopped?.Invoke(this);
        }

        public bool HasPlayer(int id)
        {
            return PlayerSessions.Exists(p => p.Peer.Id == id);
        }


        public void PlayerDisconnected(Player player)
        {
            Console.WriteLine("GameSession: PlayerDisconnected: " + player.Entity.display_name);

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
