using System;
using System.Collections.Generic;
using System.Linq;
using CardTowers_GameServer.Shine.Messages.Interfaces;
using CardTowers_GameServer.Shine.State;
using LiteNetLib;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class GameSessionHandler
    {
        private readonly Dictionary<string, GameSession> gameSessions;

        public GameSessionHandler()
        {
            gameSessions = new Dictionary<string, GameSession>();
        }

        public void AddSession(GameSession session)
        {
            gameSessions[session.Id] = session;
        }

        public void RemoveSession(GameSession session)
        {
            gameSessions.Remove(session.Id);
        }


        public void UpdateAllGameSessions()
        {
            foreach (var gameSession in gameSessions.Values)
            {
                gameSession.Update();
            }
        }

        public GameSession? GetGameSessionByPlayerId(int playerId)
        {
            return gameSessions.Values.FirstOrDefault(gs => gs.HasPlayer(playerId));
        }

        public int GetSessionCount()
        {
            return gameSessions.Count;
        }


        public void RouteGameMessage(IGameMessage gameMessage, NetPeer peer)
        {
            var sessionId = gameMessage.GameSessionId.ToString();

            if (gameSessions.ContainsKey(sessionId))
            {
                var gameSession = gameSessions[sessionId];
                gameSession.HandleGameMessage(gameMessage, peer);
            }
            else
            {
                // Handle the case where the GameSessionId is not found.
                // This could happen if the game session has ended but a client sent a message late,
                // or if there's a mistake in the message's GameSessionId.
                Console.WriteLine($"Warning: Received game message for unknown game session id {sessionId}");
            }
        }
    }
}

