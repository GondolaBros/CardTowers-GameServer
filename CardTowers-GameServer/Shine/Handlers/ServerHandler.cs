using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CardTowers_GameServer.Shine.Interfaces;
using CardTowers_GameServer.Shine.Data;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class ServerHandler
    {
        public EventBasedNetListener LiteNetListener;
        public NetManager LiteNetManager;
        public NetPacketProcessor PacketProcessor;
        public static List<Connection> ConnectedPeers = new List<Connection>();

        private volatile bool _isRunning;

        public ServerHandler()
        {
            PacketProcessor = new NetPacketProcessor();
            LiteNetListener = new EventBasedNetListener();
            LiteNetManager = new NetManager(LiteNetListener);


            LiteNetListener.NetworkReceiveEvent += LiteNetListener_NetworkReceiveEvent;
            LiteNetListener.PeerConnectedEvent += LiteNetListener_PeerConnectedEvent;
            LiteNetListener.PeerDisconnectedEvent += LiteNetListener_PeerDisconnectedEvent;
            LiteNetListener.ConnectionRequestEvent += LiteNetListener_ConnectionRequestEvent;

            MatchmakingHandler.Instance.OnMatchFound += OnMatchFound;

            RegisterAndSubscribe<MatchmakingMessage>();

            Console.WriteLine("Started connection handler...");

            _ = PeriodicMatchmakingAsync();

            Console.WriteLine("Started matchmaking handler...");
        }


      

        public void Start(int port)
        {
            LiteNetManager.Start(port);
            IsRunning = true;
            Console.WriteLine("ServerConnectionHandler listening on port: " + port);
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
                Console.WriteLine("Disconnected Peer: " + p.Peer.EndPoint.ToString() + " | ID: " + p.Id);
            }
        }


        private void OnMatchFound(object? sender, MatchFoundEventArgs e)
        {
            Console.WriteLine("Matchmaker found match for: " + e.Player1.Connection.Id
                + " | " + e.Player2.Connection.Id);

            // create the gamesession and send the details to client

            //List<Player> players = new List<Player>();

            //PlayerData p1Data = new PlayerData();
            //p1Data.Username = 

            //Player p1 = new Player(e.Player1.Connection, PlayerDataHandler.getPlayerData(e.Player1));
            //Player p2 = new Player(e.Player2.Connection, PlayerDataHandler.getPlayerData(e.Player2));

            //GameSession newGameSession = new GameSession(players);

            // old
            //MatchFoundMessage p1 = new MatchFoundMessage();
            //p1.OpponentUsername = opponent.Parameters.Username;

            //MatchFoundMessage p2 = new MatchFoundMessage();
            //p2.OpponentUsername = player.Data.Username;

            //_serverHandler.SendMessage(p1, player.Connection.Peer);
            //_serverHandler.SendMessage(p2, opponent.Connection.Peer);

            // we want to create a game session

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
            Connection? disconnectedPeer = GetPeerById(peer.Id);

            if (disconnectedPeer != null)
            {
                ConnectedPeers.Remove(disconnectedPeer);

                MatchmakingEntry matchmakingPlayer = MatchmakingHandler.Instance.GetPlayerById(disconnectedPeer.Id);
                MatchmakingHandler.Instance.TryRemove(matchmakingPlayer);

                Console.WriteLine("ConnectionHandler | PeerDisconnectedEvent: " + disconnectInfo.Reason + " | " + disconnectedPeer.Id);
            }
        }


        private void LiteNetListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            //Console.WriteLine("ConnectionHandler | ConnectionRequestEvent: " + request.ToString());
            request.AcceptIfKey("test_key");
        }


        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }
    }
}
