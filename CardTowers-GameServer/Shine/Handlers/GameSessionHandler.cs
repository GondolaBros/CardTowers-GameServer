using CardTowers_GameServer.Shine.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardTowers_GameServer.Shine.Handlers
{

    public class GameSessionHandler
    {
        private Dictionary<string, GameSession> gameSessions;

        public GameSessionHandler()
        {
            gameSessions = new Dictionary<string, GameSession>();
        }

        public void AddGameSession(GameSession gameSession)
        {
            gameSessions.Add(gameSession.Id, gameSession);
            Console.WriteLine("Added game session to manager: " + gameSession.Id);
        }

        public void RemoveGameSession(GameSession gameSession)
        {
            gameSessions.Remove(gameSession.Id);
            Console.WriteLine("Removed game session from manager: " + gameSession.Id);
        }


        public int Count()
        { 
            return gameSessions.Count;
        }

        public GameSession GetGameSession(string sessionId)
        {
            gameSessions.TryGetValue(sessionId, out GameSession gameSession);
            return gameSession;
        }


        public GameSession GetGameSessionByPlayerId(int id)
        {
            // Iterate over all game sessions to find the one that contains the player
            foreach (var gameSession in gameSessions.Values)
            {
                if (gameSession.HasPlayer(id))
                {
                    return gameSession;
                }
            }

            return null;
        }
    }
}
