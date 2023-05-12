using LiteNetLib;
using LiteNetLib.Utils;
using CardTowers_GameServer.Shine.Interfaces;
using CardTowers_GameServer.Shine.Matchmaking;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.Util;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.Data.Repositories;
using Microsoft.Extensions.Logging;
using System.Numerics;
using CardTowers_GameServer.Shine.Data.Entities;
using System.Collections.Concurrent;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class ServerHandler
    {
        public EventBasedNetListener LiteNetListener;
        public NetManager LiteNetManager;
        public NetPacketProcessor PacketProcessor;

        private MatchmakingHandler matchmakingHandler;
        private GameSessionHandler gameSessionManager;
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
            gameSessionManager = new GameSessionHandler();
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


        private void RegisterAndSubscribe<T>() where T : class, IHandledMessage, new()
        {
            PacketProcessor.SubscribeReusable<T, NetPeer>(OnMessageReceived);
            PacketProcessor.RegisterNestedType<T>(() => new T());
        }


        public void SendMessage<T>(T message, NetPeer peer) where T : class, INetSerializable, new()
        {
            NetDataWriter writer = new NetDataWriter();
            PacketProcessor.Write(writer, message);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }


        public NetPeer? GetPeerById(int id)
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
                parameters.EloRating = player.Entity.elo_rating;
                parameters.Username = player.Entity.display_name;

                MatchmakingEntry matchmakingEntry =
                    new MatchmakingEntry(player, parameters);

                matchmakingHandler.Enqueue(matchmakingEntry);
            }
            else
            {
                // handle error...
            }
        }



        private void OnMatchFound(object? sender, MatchFoundEventArgs e)
        {
            logger.LogInformation("Matchmaker found match for: " + e.P1.Player.Peer.Id
                + " | " + e.P2.Player.Peer.Id);

            GameSession newGameSession = new GameSession(e.P1, e.P2);
            newGameSession.OnGameSessionStopped += OnGameSessionStopped;
            newGameSession.Start();

            gameSessionManager.AddGameSession(newGameSession);
            GameCreatedMessage gameCreatedMessage = new GameCreatedMessage();
            gameCreatedMessage.ElapsedTicks = newGameSession.GetElapsedTime();
            gameCreatedMessage.Id = newGameSession.Id;

            SendMessage(gameCreatedMessage, e.P1.Player.Peer);
            SendMessage(gameCreatedMessage, e.P2.Player.Peer);
        }


        private void OnGameSessionStopped(GameSession gameSession)
        {
            GameEndedMessagae gameEnded = new GameEndedMessagae();
            gameEnded.ElapsedTicks = gameSession.GetElapsedTime();
            gameEnded.WinnerId = gameSession.WinnerId;

            foreach (Player p in gameSession.PlayerSessions)
            {
                this.SendMessage(gameEnded, p.Peer);
            }

            TimeSpan elapsedSpan = new TimeSpan(gameSession.GetElapsedTime());
            double totalSeconds = elapsedSpan.TotalSeconds;

            gameSession.Cleanup();

            logger.LogInformation("OnGameSessionStopped: " + gameSession.Id + " |Elapsed seconds: " + totalSeconds);

            gameSessionManager.RemoveGameSession(gameSession);
            Console.WriteLine("Total game sessions running: " + gameSessionManager.Count());
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
            //Console.WriteLine("ConnectionHandler | PeerConnectedEvent: " + peer.EndPoint.ToString());
        }


        private void LiteNetListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            logger.LogInformation("ServerHandler | PeerDisconnectedEvent: " + disconnectInfo.Reason + " | " + peer.Id);

            if (peer != null)
            {
                // Check if they were in a game
                GameSession gameSession = gameSessionManager.GetGameSessionByPlayerId(peer.Id);

                // If the peer was in a game session, handle their disconnection
                if (gameSession != null)
                {
                    Player? p = gameSession.PlayerSessions.Find(p => p.Peer.Id == peer.Id);

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
                Console.WriteLine($"Incoming client JWT is valid - accepted connection for player");
               
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
