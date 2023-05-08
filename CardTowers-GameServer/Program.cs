using CardTowers_GameServer.Shine;

namespace CardTowers_GameServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GameServer server = new GameServer();
            await server.Run();
        }
    }
}
