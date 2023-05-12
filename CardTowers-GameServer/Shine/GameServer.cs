using System.Threading.Tasks;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLib;
using CardTowers_GameServer.Shine.Handlers;

namespace CardTowers_GameServer.Shine
{
    public class GameServer
    {
        private ServerHandler serverHandler;

        public GameServer()
        {
            serverHandler = new ServerHandler();
        }

        public async Task Run()
        {
            serverHandler.Start(3456);

            while (serverHandler.IsRunning)
            {
                serverHandler.Poll();
                await Task.Delay(15);
            }
        }
    }
}
