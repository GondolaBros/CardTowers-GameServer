using LiteNetLib;
using LiteNetLib.Utils;
using CardTowers_GameServer.Shine.Matchmaking;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.Util;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.Data.Repositories;
using Microsoft.Extensions.Logging;
using System.Numerics;
using CardTowers_GameServer.Shine.Data.Entities;
using System.Collections.Concurrent;
using CardTowers_GameServer.Shine.Messages;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class ServerHandler
    {
        public EventBasedNetListener LiteNetListener;
        public NetManager LiteNetManager;
        public NetPacketProcessor PacketProcessor;

        private MatchmakingHandler matchmakingHandler;
        private GameSessionHandler gameSessionHandler;
        private CognitoJwtManager cognitoJwtManager;
        private PlayerRepository playerRepository;


        private Dictionary<NetPeer, Player> connectedPlayers = new Dictionary<NetPeer, Player>();

        private volatile bool _isRunning;

        ILogger logger;


        public ServerHandler()
        {
            var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

            // Get an ILogger from the LoggerFactory
            logger = loggerFactory.CreateLogger<ServerHandler>();

            PacketProcessor = new NetPacketProcessor();
            LiteNetListener = new EventBasedNetListener();
            LiteNetManager = new NetManager(LiteNetListener);
            matchmakingHandler = new MatchmakingHandler(this, logger);
            gameSessionHandler = new GameSessionHandler();
            cognitoJwtManager = new CognitoJwtManager(Constants.COGNITO_POOL_ID, Constants.COGNITO_REGION);
            playerRepository = new PlayerRepository(Constants.PGSQL_RDS_CONNECTION_STRING, logger);

            LiteNetListener.NetworkReceiveEvent += LiteNetListener_NetworkReceiveEvent;
            LiteNetListener.PeerConnectedEvent += LiteNetListener_PeerConnectedEvent;
            LiteNetListener.PeerDisconnectedEvent += LiteNetListener_PeerDisconnectedEvent;
            LiteNetListener.ConnectionRequestEvent += LiteNetListener_ConnectionRequestEvent;

            matchmakingHandler.OnMatchFound += OnMatchFound;
            NetEvents.OnMatchmakingEntryReceived += NetEvents_OnMatchmakingEntryReceived;

            RegisterAndSubscribe<MatchmakingMessage>();
            RegisterAndSubscribe<GameCreatedMessage>();
            RegisterAndSubscribe<GameEndedMessagae>();
        }
        

        public void Start(int port)
        {
            LiteNetManager.Start(port);
            IsRunning = true;
            logger.LogInformation("ServerHandler listening on port: " + port);
        }


        public void Stop()
        {
            LiteNetManager.Stop();
            IsRunning = false;
        }


        public void Poll()
        {
            LiteNetManager.PollEvents();
        }


        public void Update()
        {
            this.gameSessionHandler.UpdateAllGameSessions();
        }


        private void RegisterAndSubscribe<T>() where T : class, IHandledMessage, new()
        {
            PacketProcessor.SubscribeReusable<T, NetPeer>(OnMessageReceived);
            PacketProcessor.RegisterNestedType<T>(() => new T());
        }


        public void SendMessage<T>(T message, NetPeer peer, bool sendUnreliable) where T : class, IHandledMessage, new()
        {
            NetDataWriter writer = new NetDataWriter();
            PacketProcessor.Write(writer, message);
            peer.Send(writer, sendUnreliable ? DeliveryMethod.Unreliable : DeliveryMethod.ReliableOrdered);
        }


        public NetPeer GetPeerById(int id)
        {
            return this.LiteNetManager.ConnectedPeerList.Find(p => p.Id == id);
        }


        public void Disconnect(NetPeer peer)
        {
            NetPeer? p = GetPeerById(peer.Id);

            if (p != null)
            {
                p.Disconnect();
            }
        }


        private void NetEvents_OnMatchmakingEntryReceived(MatchmakingEntryReceivedEventArgs obj)
        {
            // If this peer is already connected
            Player player;
            if (connectedPlayers.TryGetValue(obj.Peer, out player))
            {
                MatchmakingParameters parameters = new MatchmakingParameters();
                parameters.Id = player.Peer.Id;
                parameters.EloRating = player.Entity.elo_rating;
                parameters.Username = player.Entity.display_name;

                MatchmakingEntry matchmakingEntry =
                    new MatchmakingEntry(parameters);

                matchmakingHandler.Enqueue(matchmakingEntry);
            }
            else
            {
                // handle error...


            }
        }


        private void OnMatchFound(object? sender, MatchFoundEventArgs e)
        {
            logger.LogInformation("Matchmaker found match for: " + e.P1.Parameters.Id
                + " | " + e.P2.Parameters.Id);

            Player? p1;
            Player? p2;

            if (connectedPlayers.TryGetValue(GetPeerById(e.P1.Parameters.Id), out p1)
                && connectedPlayers.TryGetValue(GetPeerById(e.P2.Parameters.Id), out p2))
            {
                try
                {
                    GameSession newGameSession = new GameSession();
                    newGameSession.AddPlayer(p1);
                    newGameSession.AddPlayer(p2);

                    newGameSession.OnGameSessionStopped += OnGameSessionStopped;

                    gameSessionHandler.AddSession(newGameSession);
                    GameCreatedMessage gameCreatedMessage = new GameCreatedMessage();
                    gameCreatedMessage.ElapsedTicks = newGameSession.GetElapsedTime();
                    gameCreatedMessage.Id = newGameSession.Id;

                    SendMessage(gameCreatedMessage, GetPeerById(e.P1.Parameters.Id), false);
                    SendMessage(gameCreatedMessage, GetPeerById(e.P2.Parameters.Id), false);
                }

                catch (Exception ex)
                {
                    logger.LogError("Found exception when trying to create game session: " + ex.ToString());
                }
            }
        }


        private void OnGameSessionStopped(GameSession gameSession)
        {
            GameEndedMessagae gameEnded = new GameEndedMessagae();
            gameEnded.ElapsedTicks = gameSession.GetElapsedTime();
            gameEnded.WinnerId = gameSession.WinnerId;

            foreach (Player p in gameSession.PlayerStates.Keys)
            {
                this.SendMessage(gameEnded, p.Peer, false);
            }

            TimeSpan elapsedSpan = new TimeSpan(gameSession.GetElapsedTime());
            double totalSeconds = elapsedSpan.TotalSeconds;

            gameSession.Cleanup();

            logger.LogInformation("OnGameSessionStopped: " + gameSession.Id + " | Elapsed seconds: " + totalSeconds);

            gameSessionHandler.RemoveSession(gameSession);
            logger.LogInformation("Total game sessions running: " + gameSessionHandler.Count());
        }


        private void OnMessageReceived<T>(T message, NetPeer peer) where T : IHandledMessage
        {
            message.Handle(peer);
        }


        private void LiteNetListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            PacketProcessor.ReadAllPackets(reader, peer);
        }


        private void LiteNetListener_PeerConnectedEvent(NetPeer peer)
        {
            // TODO: what now in peerconnectedevent

        }


        private void LiteNetListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            logger.LogInformation("ServerHandler | PeerDisconnectedEvent: " + disconnectInfo.Reason + " | " + peer.Id);

            if (peer != null)
            {
                // Check if they were in a game
                GameSession? gameSession = gameSessionHandler.GetGameSessionByPlayerId(peer.Id);

                // If the peer was in a game session, handle their disconnection
                if (gameSession != null)
                {
                    Player? p = gameSession.PlayerStates.Keys.FirstOrDefault(p => p.Peer.Id == peer.Id);

                    if (p != null)
                    {
                        // right now call player disconnected. itll handle game session
                        // eventually we should rename this to like
                        // cleanupgamesesison or sumn along the lines xd
                        gameSession.PlayerDisconnected(p);
                    }
                }


                // Check if they were matchmaking
                MatchmakingEntry? potentialEntry = matchmakingHandler.GetMatchmakingEntryById(peer.Id);

                if (potentialEntry != null)
                {
                    matchmakingHandler.TryRemove(potentialEntry);
                }

                connectedPlayers.Remove(peer);
            }
        }


        private async void OnConnectionRequestAsync(ConnectionRequest request)
        {
            // The client sends the jwt as a string from client connection request
            string token = request.Data.GetString();

            // Validate the token
            bool isValid = await cognitoJwtManager.ValidateTokenAsync(token);

            if (isValid)
            {
                logger.LogInformation("Incoming client JWT is valid - accepted connection for player: " + request.RemoteEndPoint.ToString());
               
                // If the token is valid, get the "sub" claim which is the account id
                string? accountId = cognitoJwtManager.GetSubjectFromToken(token);

                if (accountId != null)
                {
                    string? username = cognitoJwtManager.GetUsernameFromToken(token);

                    if (username != null)
                    {
                        var playerEntity = await playerRepository.LoadOrCreatePlayerAccount(accountId, username);

                        NetPeer peer = request.Accept();
                        Player player = new Player(peer, playerEntity);

                        connectedPlayers.Add(peer, player);
                        
                        return;
                    }
                }
            }

            // reject on any case, no exceptins - this is critical!
            request.Reject();
        }


        private void LiteNetListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            OnConnectionRequestAsync(request);
        }


        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }
    }
}
