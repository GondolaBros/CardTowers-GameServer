using CardTowers_GameServer.Shine.Data;
using CardTowers_GameServer.Shine.Network;

namespace CardTowers_GameServer.Shine.Models
{
    public class Player
    {
        // Underlying network transport connection
        public Connection Connection { get; private set; }

        // Accounnt persistent data
        public PlayerData Data { get; private set; }

        // Realtime data
        public GameMap GameMap { get; private set; }

        public Deck Deck { get; private set; }

        public Player(Connection connection, PlayerData playerData)
        {
            Connection = connection;
            Data = playerData;
        }
    }
}

