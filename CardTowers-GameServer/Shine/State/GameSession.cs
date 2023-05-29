using System;
using System.Collections.Generic;
using System.Diagnostics;
using CardTowers_GameServer.Shine.Data;
using CardTowers_GameServer.Shine.Handlers;
using CardTowers_GameServer.Shine.Matchmaking;
using CardTowers_GameServer.Shine.Messages.Interfaces;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.State.Deltas;
using CardTowers_GameServer.Shine.Util;
using LiteNetLib;
using LiteNetLib.Utils;

namespace CardTowers_GameServer.Shine.State
{
    public class GameSession
    {
        public string Id { get; private set; }

        public Stopwatch ServerStopwatch { get; private set; }
        public Dictionary<Player, PlayerState> PlayerStates { get; private set; }
        public event Action<GameSession> OnGameSessionStopped;

        private long lastTickTime;
        private long accumulatedDeltaTime;

        public int WinnerId { get; private set; }

        bool started = false;

        private readonly ServerHandler serverHandler;
        private readonly GameMessageSerializer gameMessageSerializer;

        public GameSession(ServerHandler serverHandler, GameMessageSerializer gameMessageSerializer)
        {
            Id = Guid.NewGuid().ToString();

            this.serverHandler = serverHandler;
            this.gameMessageSerializer = gameMessageSerializer;

            PlayerStates = new Dictionary<Player, PlayerState>();

        }


        public void Start()
        {
            ServerStopwatch = Stopwatch.StartNew();
            lastTickTime = ServerStopwatch.ElapsedMilliseconds;
            accumulatedDeltaTime = 0;
            started = true;
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


        public void AddPlayer(Player player)
        {
            var playerState = new PlayerState(this.Id);
            PlayerStates.Add(player, playerState);
        }


        public void RemovePlayer(Player player)
        {
            PlayerStates.Remove(player);
        }


        public bool HasPlayer(int id)
        {
            foreach (var player in PlayerStates.Keys)
            {
                if (player.Peer.Id == id)
                    return true;
            }
            return false;
        }


        public void Update()
        {
            if (started)
            {
                long currentTickTime = ServerStopwatch.ElapsedMilliseconds;
                long deltaTime = currentTickTime - lastTickTime;

                foreach (var playerState in PlayerStates.Values)
                {
                    playerState.ComponentStateHandler.Update(deltaTime);
                }

                lastTickTime = currentTickTime;
            }
        }


        public void HandleGameMessage(IGameMessage gameMessage, NetPeer peer)
        {
            if (this.HasPlayer(peer.Id))
            {
                PlayerState playerState = this.PlayerStates.Where(p => p.Key.Peer.Id == peer.Id).FirstOrDefault().Value;
                playerState.ComponentStateHandler.ReceiveAndApplyDelta(gameMessage);
            }
        }


        public long GetElapsedTime()
        {
            return ServerStopwatch.ElapsedMilliseconds;
        }


        public void PlayerDisconnected(Player player)
        {
            Console.WriteLine("GameSession: PlayerDisconnected: " + player.Entity.display_name);

            // Handle player disconnection, possibly end the game and declare the other player as the winner.
            // Remove the player from the session
            PlayerStates.Remove(player);

            // If there's only one player left, they are the winner
            if (PlayerStates.Count == 1)
            {
                Console.WriteLine("GameSession player count: " + PlayerStates.Count);
                Stop(PlayerStates.Keys.GetEnumerator().Current);
            }
        }


        public void Cleanup()
        {
            PlayerStates.Clear();
        }
    }
}
