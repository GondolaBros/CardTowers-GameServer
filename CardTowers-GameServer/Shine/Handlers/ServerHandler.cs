using CardTowers_GameServer.Shine.Matchmaking;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.Util;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.Data.Repositories;
using CardTowers_GameServer.Shine.Data.Entities;
using CardTowers_GameServer.Shine.Messages;
using CardTowers_GameServer.Shine.Messages.Interfaces;

using LiteNetLib;
using LiteNetLib.Utils;

using Microsoft.Extensions.Logging;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class ServerHandler
    {
        public EventBasedNetListener LiteNetListener;
        public NetManager LiteNetManager;
        public NetPacketProcessor PacketProcessor;

        private GameMessageSerializer gameMessageSerializer;

        private MatchmakingHandler matchmakingHandler;
        private GameSessionHandler gameSessionHandler;
        private CognitoJwtManager cognitoJwtManager;
        private PlayerRepository playerRepository;
        private EventDispatcher eventDispatcher;

        private Dictionary<NetPeer, Player> validatedPlayers = new Dictionary<NetPeer, Player>();

        private volatile bool _isRunning;

        ILogger logger;

        public ServerHandler()
        {
            var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

            logger = loggerFactory.CreateLogger<ServerHandler>();

            eventDispatcher = new EventDispatcher();
            PacketProcessor = new NetPacketProcessor();
            gameMessageSerializer = new GameMessageSerializer();
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

            RegisterAndSubscribe<MatchmakingMessage>(MessageChannel.System);
            RegisterAndSubscribe<GameCreatedMessage>(MessageChannel.System);
            RegisterAndSubscribe<GameEndedMessagae>(MessageChannel.System);

            RegisterAndSubscribe<ManaDeltaMessage>(MessageChannel.Game);
        }


        /// <summary>
        /// Flag to determine if the server is running or not
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }


        /// <summary>
        /// A method to start the server and logic thread, and have it listen on specified port.
        /// </summary>
        /// <param name="port">The port the server is listening on</param>
        public void Start(int port)
        {
            LiteNetManager.Start(port);
            IsRunning = true;
            logger.LogInformation("ServerHandler listening on port: " + port);
        }


        /// <summary>
        /// This method simply stops the server. It force closes connections
        /// and stops all threads.
        /// </summary>
        public void Stop()
        {
            LiteNetManager.Stop();
            IsRunning = false;
        }


        /// <summary>
        /// This method is called from main game server loop and polls all network events
        /// internally from litenetlib. 
        /// </summary>
        public void Poll()
        {
            LiteNetManager.PollEvents();
        }


        /// <summary>
        /// This method is also called from the main game server loop
        /// and updates all the game sessions.
        /// </summary>
        public void Update()
        {
            this.gameSessionHandler.UpdateAllGameSessions();
        }


        /// <summary>
        /// This is a helper method that searches for a peer by its ID and returns
        /// the corresponding NetPeer object. This ID is handled automatically by LiteNetLib.
        /// </summary>
        /// <param name="id">The id of the NetPeer</param>
        /// <returns>The NetPeer belonging to the ID.</returns>
        public NetPeer GetPeerById(int id)
        {
            return this.LiteNetManager.ConnectedPeerList.Find(p => p.Id == id);
        }


        /// <summary>
        /// The method simply disconnects a peer with no additional information.
        /// </summary>
        /// <param name="peer"></param>
        public void Disconnect(NetPeer peer)
        {
            //NetPeer? p = GetPeerById(peer.Id);
            peer.Disconnect();
        }


        /// <summary>
        /// This method disconnects the specified NetPeer and writes additional
        /// data that will be sent to the client. This can be useful if we need to 
        /// </summary>
        /// <param name="peer">The peer to disconnect</param>
        /// <param name="writer">Arbitrary data to include which will be sent to client</param>
        public void Disconnect(NetPeer peer, NetDataWriter writer)
        {
            peer.Disconnect(writer);
        }


        /// <summary>
        /// This method sends a system message to a validated peer.
        /// </summary>
        /// <typeparam name="T">A generic message type of ISystemMessage. Any message
        /// that implements ISystemMessage can use this function.</typeparam>
        /// <param name="message">The ISystemMessage instance</param>
        /// <param name="peer">The peer to send the message to</param>
        /// <param name="sendUnreliable">True to send the message as unreliable, and False to send reliably.</param>
        public void SendMessageWPacketProcessor<T>(T message, NetPeer peer, bool sendUnreliable) where T : class, ISystemMessage, new()
        {
            NetDataWriter writer = new NetDataWriter();
            PacketProcessor.Write(writer, message);
            peer.Send(writer, sendUnreliable ? DeliveryMethod.Unreliable : DeliveryMethod.ReliableOrdered);
        }


        /// <summary>
        /// This method registers and subscribes specific messages to their handlers and call back functions.
        /// At the moment we have two types of INetworkMessages: IGameMessage and ISystemMessage. We need to handle
        /// IGameMessages differently due to routing, so we have a custom event dispatcher which
        /// dispatches game messages to their corresponding GameSession.
        /// ISystemMessages are handled through LiteNetLib's packet processr at the moment, since we're
        /// automatically serializing/deserializing those messages. 
        /// </summary>
        /// <typeparam name="T">Generic type of INetworkMessage. IGameMessage & ISystemMessage
        /// both implement INetworkMessage.</typeparam>
        /// <param name="channel">The byte channels which messages are sent over.</param>
        public void RegisterAndSubscribe<T>(MessageChannel channel) where T : class, INetworkMessage, new()
        {
            byte byteChannel = (byte)channel;

            if (typeof(ISystemMessage).IsAssignableFrom(typeof(T)))
            {
                PacketProcessor.SubscribeReusable<T, NetPeer>((message, peer) => OnSystemMessageReceived((ISystemMessage)message, peer));
                PacketProcessor.RegisterNestedType<T>(() => new T());
            }
            else if (typeof(IGameMessage).IsAssignableFrom(typeof(T)))
            {
                eventDispatcher.RegisterGameMessageHandler<IGameMessage>((message, peer) => OnGameMessageReceived((IGameMessage)message, peer), byteChannel);
            }
        }


        /// <summary>
        /// This callback is invoked internally by LiteNetLib when the network receives messages.
        /// First we determine which channel the messages belong to. Then handle them accordingly.
        /// </summary>
        /// <param name="peer">The peer who sent the message</param>
        /// <param name="reader">The message data which is / will be serialized</param>
        /// <param name="channel">The channel the messages belong to</param>
        /// <param name="deliveryMethod">The delivery method of the message</param>
        private void LiteNetListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            if (channel == (byte)MessageChannel.System)
            {
                PacketProcessor.ReadAllPackets(reader, peer);
            }
            else if (channel == (byte)MessageChannel.Game)
            {
                IGameMessage gameMessage = gameMessageSerializer.Deserialize(reader);
                if (gameMessage != null)
                {
                    eventDispatcher.DispatchGameMessage(gameMessage, peer, channel);
                }
            }
        }


        /// <summary>
        /// This callback is invoked automatically from our event dispatcher.
        /// The reason we cant use LiteNetLib packet processor is because our
        /// IGameMessage require custom serializing and routing to game session
        /// and specific components.
        /// </summary>
        /// <param name="gameMessage">The concrete game message</param>
        /// <param name="peer">The client that sent the game message</param>
        private void OnGameMessageReceived(IGameMessage gameMessage, NetPeer peer)
        {
            gameSessionHandler.RouteGameMessage(gameMessage, peer);
        }


        /// <summary>
        /// This method is invoked from litenetlibs packet processor when we receive ISystemMessages
        /// and then handles it accordingly.
        /// </summary>
        /// <param name="systemMessage">The specific system message received</param>
        /// <param name="peer">The peer who sent the system message</param>
        private void OnSystemMessageReceived(ISystemMessage systemMessage, NetPeer peer)
        {
            systemMessage.Handle(peer);
        }


        /// <summary>
        /// Invokes the aysnc connection request method.
        /// </summary>
        /// <param name="request">Base connection request</param>
        private void LiteNetListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            OnConnectionRequestAsync(request);
        }


        /// <summary>
        /// Our client sends a connection request using litenetlib. We parse
        /// the JWT in the connection request, and then validate the clients identity using
        /// the cognito jwt manager. If validated, we will load or create their
        /// game account from database. Upon success, we will then accept the clients
        /// connection, upon failure, we will simply reject their connection request.
        /// </summary>
        /// <param name="request">Litenetlib connection request</param>
        private async void OnConnectionRequestAsync(ConnectionRequest request)
        {
            string token = request.Data.GetString();
            bool isValid = await cognitoJwtManager.ValidateTokenAsync(token);

            if (isValid)
            {
                string? accountId = cognitoJwtManager.GetSubjectFromToken(token);

                if (accountId != null)
                {
                    string? username = cognitoJwtManager.GetUsernameFromToken(token);

                    if (username != null)
                    {
                        PlayerEntity? playerEntity = await playerRepository.LoadOrCreatePlayerAccount(accountId, username);

                        if (playerEntity != null)
                        {
                            NetPeer peer = request.Accept();
                            Player player = new Player(peer, playerEntity);
                            validatedPlayers.Add(peer, player);

                            logger.LogInformation("Incoming client JWT is valid - accepted connection for player: " + request.RemoteEndPoint.ToString());

                            return;
                        }
                    }
                }
            }
            request.Reject();
        }



        /// <summary>
        /// This callback is invoked when a client sends a find match request. It will
        /// create matchmaking parameters based on the player, and then enqueue them into
        /// the matchmaker.
        /// </summary>
        /// <param name="obj">Information about the peer who initially sent the request</param>
        private void NetEvents_OnMatchmakingEntryReceived(MatchmakingEntryReceivedEventArgs obj)
        {
            Player player;
            if (validatedPlayers.TryGetValue(obj.Peer, out player))
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
                // TODO: real error handling
                throw new Exception("OnMatchmakingEntryReceived: Client illegally manipulated. Client is not a validated player!");
            }
        }


        /// <summary>
        /// This callback is invoked automatically by the matchmaking service when a match
        /// has been found for 2 players.
        /// </summary>
        /// <param name="sender">The object that invoked the callback</param>
        /// <param name="e">The players who were matched from the matchmaker, and their corresponding data</param>
        private void OnMatchFound(object? sender, MatchFoundEventArgs e)
        {
            logger.LogInformation("Matchmaker found match for: " + e.P1.Parameters.Id
                + " | " + e.P2.Parameters.Id);

            Player? p1;
            Player? p2;

            if (validatedPlayers.TryGetValue(GetPeerById(e.P1.Parameters.Id), out p1)
                && validatedPlayers.TryGetValue(GetPeerById(e.P2.Parameters.Id), out p2))
            {
                try
                {
                    GameSession newGameSession = new GameSession(this, gameMessageSerializer);
                    newGameSession.AddPlayer(p1);
                    newGameSession.AddPlayer(p2);
                    newGameSession.Start();

                    newGameSession.OnGameSessionStopped += OnGameSessionStopped;

                    gameSessionHandler.AddSession(newGameSession);

                    GameCreatedMessage gameCreatedMessage = new GameCreatedMessage();
                    gameCreatedMessage.ElapsedTicks = newGameSession.GetElapsedTime();
                    logger.LogInformation("Elapsed ticks: " + gameCreatedMessage.ElapsedTicks);
                    gameCreatedMessage.Id = newGameSession.Id;

                    SendMessageWPacketProcessor(gameCreatedMessage, GetPeerById(e.P1.Parameters.Id), false);
                    SendMessageWPacketProcessor(gameCreatedMessage, GetPeerById(e.P2.Parameters.Id), false);
                }
                catch (Exception ex)
                {
                    logger.LogError("Found exception when trying to create game session: " + ex.ToString());
                }
            }
        }


        /// <summary>
        /// This callback is invoked internally when a game session has ended.
        /// Useful for handling end game logic and cleaning up game session if need.
        /// </summary>
        /// <param name="gameSession">The game session that ended</param>
        private void OnGameSessionStopped(GameSession gameSession)
        {
            GameEndedMessagae gameEnded = new GameEndedMessagae();
            gameEnded.ElapsedTicks = gameSession.GetElapsedTime();
            gameEnded.WinnerId = gameSession.WinnerId;

            foreach (Player p in gameSession.PlayerStates.Keys)
            {
                this.SendMessageWPacketProcessor(gameEnded, p.Peer, false);
            }

            double elapsedTime = TimeHelper.MillisecondsToSeconds(gameSession.GetElapsedTime());

            //TimeSpan elapsedSpan = new TimeSpan(gameSession.GetElapsedTime());
            //double totalSeconds = elapsedSpan.TotalSeconds;

            gameSession.Cleanup();
            gameSessionHandler.RemoveSession(gameSession);

            logger.LogInformation("OnGameSessionStopped: " + gameSession.Id + " | Elapsed seconds: " + elapsedTime);
            logger.LogInformation("Total game sessions running: " + gameSessionHandler.GetSessionCount());
        }


        /// <summary>
        /// This method is invoked automatically by litenetlib when a peer disconnects from the server.
        /// It will determine if a player is in a game session, or if they're potentially matchmaking,
        /// and remove them and clean up after them if they are.
        /// </summary>
        /// <param name="peer">The peer that disconnected</param>
        /// <param name="disconnectInfo">Information regarding the peers disconnection</param>
        private void LiteNetListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            logger.LogInformation("ServerHandler | PeerDisconnectedEvent: " + disconnectInfo.Reason + " | " + peer.Id);

            if (peer != null)
            {
                GameSession? gameSession = gameSessionHandler.GetGameSessionByPlayerId(peer.Id);

                if (gameSession != null)
                {
                    Player? p = gameSession.PlayerStates.Keys.FirstOrDefault(p => p.Peer.Id == peer.Id);

                    if (p != null)
                    {
                        // TODO: properly handle game session disconnects.
                        logger.LogInformation("trying to remove player from game session: " + p);
                        gameSession.PlayerDisconnected(p);
                    }
                }

                MatchmakingEntry? potentialEntry = matchmakingHandler.GetMatchmakingEntryById(peer.Id);

                if (potentialEntry != null)
                {
                    matchmakingHandler.TryRemove(potentialEntry);
                }

                validatedPlayers.Remove(peer);
            }
        }


        /// <summary>
        /// This method is invoked automatically by Litenetlib, however we have no implementation for it yet.
        /// However, we could use this to handle player reconnections or other state restoration logic
        /// </summary>
        /// <param name="peer"></param>
        private void LiteNetListener_PeerConnectedEvent(NetPeer peer)
        {
            // TODO: what now in peerconnectedevent
        }
    }
}
