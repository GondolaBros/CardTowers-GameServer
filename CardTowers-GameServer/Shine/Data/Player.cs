using System;
using CardTowers_GameServer.Shine.Models;

namespace CardTowers_GameServer.Shine.Data
{
    public class Player
    {
        // Underlying network transport connection
        public Connection Connection { get; private set; }

        // Accounnt persistent data
        public PlayerData Data { get; private set; }

        // Realtime data
        public GameMap GameMap { get; private set; }

        public Player(Connection connection, PlayerData playerData)
        {
            this.Connection = connection;
            this.Data = playerData;
        }
    }
}

