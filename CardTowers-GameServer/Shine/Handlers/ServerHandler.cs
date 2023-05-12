using LiteNetLib;
using LiteNetLib.Utils;
using CardTowers_GameServer.Shine.Interfaces;
using CardTowers_GameServer.Shine.Matchmaking;
using CardTowers_GameServer.Shine.Network;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.Util;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.Data.Repositories;
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class ServerHandler
    {
        public EventBasedNetListener LiteNetListener;
        public NetManager LiteNetManager;
        public NetPacketProcessor PacketProcessor;
        private GameSessionHandler gameSessionManager;
        private CognitoJwtManager cognitoJwtManager;
        public static List<Connection> ConnectedPeers = new List<Connection>();
        private PlayerRepository playerRepository;

        private volatile bool _isRunning;

        public ServerHandler()
        {
            var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

            PacketProcessor = new NetPacketProcessor();
            LiteNetListener = new EventBasedNetListener();
            LiteNetManager = new NetManager(LiteNetListener);
            gameSessionManager = new GameSessionHandler();
            cognitoJwtManager = new CognitoJwtManager(Constants.COGNITO_POOL_ID, Constants.COGNITO_REGION);

            // Get an ILogger from the LoggerFactory
            ILogger logger = loggerFactory.CreateLogger<ServerHandler>();

            playerRepository = new PlayerRepository(Constants.PGSQL_RDS_CONNECTION_STRING, logger);

            LiteNetListener.NetworkReceiveEvent += LiteNetListener_NetworkReceiveEvent;
            LiteNetListener.PeerConnectedEvent += LiteNetListener_PeerConnectedEvent;
            LiteNetListener.PeerDisconnectedEvent += LiteNetListener_PeerDisconnectedEvent;
            LiteNetListener.ConnectionRequestEvent += LiteNetListener_ConnectionRequestEvent;

            MatchmakingHandler.Instance.OnMatchFound += OnMatchFound;

            RegisterAndSubscribe<MatchmakingMessage>();
            RegisterAndSubscribe<GameCreatedMessage>();
            RegisterAndSubscribe<GameEndedMessagae>();

            _ = PeriodicMatchmakingAsync();

            Console.WriteLine("Started connection handler...");
            Console.WriteLine("Started matchmaking handler...");
            Console.WriteLine("Started game session handler...");

        }



        public void Start(int port)
        {
            LiteNetManager.Start(port);
            IsRunning = true;
            Console.WriteLine("ServerHandler listening on port: " + port);
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



        public static Connection? GetPeerById(int id)
        {
            return ConnectedPeers.Find(p => p.Id == id);
        }



        public void Disconnect(Connection peer)
        {
            Connection? p = GetPeerById(peer.Id);

            if (p != null)
            {
                p.Peer.Disconnect();
                ConnectedPeers.Remove(p);
            }
        }


        private void OnMatchFound(object? sender, MatchFoundEventArgs e)
        {
            Console.WriteLine("Matchmaker found match for: " + e.Player1.Connection.Id
                + " | " + e.Player2.Connection.Id);

            GameSession newGameSession = new GameSession(e.Player1, e.Player2);
            newGameSession.OnGameSessionStopped += OnGameSessionStopped;
            newGameSession.Start();

            gameSessionManager.AddGameSession(newGameSession);
            GameCreatedMessage gameCreatedMessage = new GameCreatedMessage();
            gameCreatedMessage.ElapsedTicks = newGameSession.GetElapsedTime();
            gameCreatedMessage.Id = newGameSession.Id;

            SendMessage(gameCreatedMessage, e.Player1.Connection.Peer);
            SendMessage(gameCreatedMessage, e.Player2.Connection.Peer);
        }


        private void OnGameSessionStopped(GameSession gameSession)
        {
            GameEndedMessagae gameEnded = new GameEndedMessagae();
            gameEnded.ElapsedTicks = gameSession.GetElapsedTime();
            gameEnded.WinnerId = gameSession.WinnerId;

            foreach (Player p in gameSession.PlayerSessions)
            {
                this.SendMessage(gameEnded, p.Connection.Peer);
            }

            TimeSpan elapsedSpan = new TimeSpan(gameSession.GetElapsedTime());
            double totalSeconds = elapsedSpan.TotalSeconds;

            gameSession.Cleanup();

            Console.WriteLine("OnGameSessionStopped: " + gameSession.Id + " |Elapsed seconds: " + totalSeconds);

            gameSessionManager.RemoveGameSession(gameSession);
            Console.WriteLine("Total game sessions running: " + gameSessionManager.Count());
        }

        public async Task PeriodicMatchmakingAsync()
        {
            await MatchmakingHandler.Instance.PeriodicMatchmakingAsync();
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
            Console.WriteLine("ConnectionHandler | PeerConnectedEvent: " + peer.EndPoint.ToString());
            Connection connectedPeer = new Connection(peer);
            ConnectedPeers.Add(connectedPeer);
        }


        private void LiteNetListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine("ServerHandler | PeerDisconnectedEvent: " + disconnectInfo.Reason + " | " + peer.Id);

            Connection? disconnectedPeer = GetPeerById(peer.Id);

           if (disconnectedPeer != null)
           {
                // Check if they were in a game
                GameSession gameSession = gameSessionManager.GetGameSessionByPlayerId(disconnectedPeer.Id);

                // If the peer was in a game session, handle their disconnection
                if (gameSession != null)
                {
                    Player? p = gameSession.PlayerSessions.Find(p => p.Connection.Id == disconnectedPeer.Id);

                    if (p != null)
                    { 
                        gameSession.PlayerDisconnected(p);
                    }

                }

                // Check if they were matchmaking
                MatchmakingEntry? potentialEntry = MatchmakingHandler.Instance.GetPlayerById(disconnectedPeer.Id);

                if (potentialEntry != null)
                {
                    MatchmakingHandler.Instance.TryRemove(potentialEntry);
                }

           
                // Now finally clean up their connection
                ConnectedPeers.Remove(disconnectedPeer);
            }
        }


        private async void OnConnectionRequestAsync(ConnectionRequest request)
        {
            // Assuming the JWT token is sent as a string in the ConnectionRequest's Data
            string token = request.Data.GetString();

            // Validate the token
            bool isValid = await cognitoJwtManager.ValidateTokenAsync(token);

            if (isValid)
            {
                Console.WriteLine($"Incoming client JWT is valid - accepted connection for player");

                try
                {

                    // If the token is valid, get the "sub" claim which is the account id
                    string? accountId = cognitoJwtManager.GetSubjectFromToken(token);

                    if (accountId != null)
                    {
                        string? username = cognitoJwtManager.GetUsernameFromToken(token);

                        if (username != null)
                        {
                            // Load or create the player account
                            var player = await playerRepository.LoadOrCreatePlayerAccount(accountId, username);
                            // TODO: Here you might want to associate the player with the connection somehow,
                            // so you know which player is associated with each connection.
                            // For example, you might want to create a dictionary where the key is the connection
                            // and the value is the player.

                            // Accept the connection
                            request.Accept();
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught exception inside repository: " + e.Message.ToString());
                }
            }
            else
            {
                // If the token is not valid, reject the connection
                request.Reject();
            }
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
