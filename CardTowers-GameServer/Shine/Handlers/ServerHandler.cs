using LiteNetLib;
using LiteNetLib.Utils;
using CardTowers_GameServer.Shine.Interfaces;
using CardTowers_GameServer.Shine.Matchmaking;
using CardTowers_GameServer.Shine.Entities;
using CardTowers_GameServer.Shine.Network;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.Util;

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
        private volatile bool _isRunning;

        public ServerHandler()
        {
            PacketProcessor = new NetPacketProcessor();
            LiteNetListener = new EventBasedNetListener();
            LiteNetManager = new NetManager(LiteNetListener);
            gameSessionManager = new GameSessionHandler();
            cognitoJwtManager = new CognitoJwtManager(Constants.COGNITO_POOL_ID, Constants.COGNITO_REGION);

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
                // If the token is valid, accept the connection
                request.Accept();
                Console.WriteLine("Incoming client JWT is valid - accepted connection");
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
