using System;
using System.Collections.Generic;
using System.Diagnostics;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.Util;

namespace CardTowers_GameServer.Shine.Data
{
    public class GameSession
    {
        public string Id { get; private set; }
        public Dictionary<Player, GameMap> PlayerGameMap { get; private set; }
        public Stopwatch ElapsedTime { get; private set; }

        private readonly List<Player> playerSessions;

        // Constructor
        public GameSession(List<Player> players)
        {
            this.Id = Guid.NewGuid().ToString();
            this.playerSessions = players;

            PlayerGameMap = new Dictionary<Player, GameMap>();

            // Initialize each player's game map
            foreach (var player in playerSessions)
            {
                PlayerGameMap[player] = new GameMap();
            }

            ElapsedTime = new Stopwatch();

        }


        // Assign a map to a player
        public void AssignPlayerToMap(Player player, GameMap map)
        {
            PlayerGameMap[player] = map;
        }

        // Get a player's map
        public GameMap GetPlayerMap(Player player)
        {
            return PlayerGameMap[player];
        }


        // Methods to manage the game session
        public void StartGame()
        {
            // Initialize game state, send initial data to players, start the stopwatch, etc.
            ElapsedTime.Start();
        }

        public void EndGame(Player winner)
        {
            // Handle game end logic, declare the winner, send final data to players, etc.
            ElapsedTime.Stop();
        }

        public void UpdateGame()
        {
            // Update game state, process player inputs, update the game board for each player, etc.
        }

        public void PlayerDisconnected(Player player)
        {
            // Handle player disconnection, possibly end the game and declare the other player as the winner.
        }
    }
}

