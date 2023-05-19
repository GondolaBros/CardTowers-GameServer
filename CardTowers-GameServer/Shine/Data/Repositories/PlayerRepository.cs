using CardTowers_GameServer.Shine.Data.Entities;
using Microsoft.Extensions.Logging;

namespace CardTowers_GameServer.Shine.Data.Repositories
{
    public class PlayerRepository : BaseRepository<PlayerEntity>
    {
        ILogger logger;

        public PlayerRepository(string connectionString, ILogger logger)
            : base(connectionString, "player", logger)
        {
            this.logger = logger;
        }


        public async Task<PlayerEntity?> LoadOrCreatePlayerAccount(string accountId, string username)
        {
            PlayerEntity? player = null;
            try
            {
                player = await GetByPropertyAsync(p => p.account_id, accountId);
                if (player == null)
                {
                    player = new PlayerEntity
                    {
                        account_id = accountId,
                        display_name = username
                    };
                    await InsertAsync(player);
                }
            }
            catch (Exception e)
            {
                this.logger.LogError("Caught exception when trying to load or create player: " + e.ToString());
            }

            return player;
            
        }
    }
}

