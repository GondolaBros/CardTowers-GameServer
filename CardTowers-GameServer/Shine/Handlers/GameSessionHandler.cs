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
        private List<GameSession> gameSessions;

        public GameSessionHandler()
        {
            gameSessions = new List<GameSession>();
        }


        public void AddSession(GameSession session)
        {
            gameSessions.Add(session);
        }

        public void RemoveSession(GameSession session)
        {
            gameSessions.Remove(session);
        }


        public void UpdateAllGameSessions()
        {
            foreach (var gameSession in gameSessions)
            {
                gameSession.Update();
            }
        }


        public GameSession? GetGameSessionByPlayerId(int id)
        {
            return gameSessions.FirstOrDefault(gs => gs.HasPlayer(id));
        }


        public int Count()
        {
            return this.gameSessions.Count;
        }
    }
}
