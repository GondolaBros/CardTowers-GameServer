using CardTowers_GameServer.Shine.Data.Entities;
using Microsoft.Extensions.Logging;

namespace CardTowers_GameServer.Shine.Data.Repositories
{
    public class PlayerRepository : BaseRepository<PlayerEntity>
    {
        public PlayerRepository(string connectionString, ILogger logger)
            : base(connectionString, "player", logger)
        {
        }


        public async Task<PlayerEntity> LoadOrCreatePlayerAccount(string accountId, string username)
        {
            var player = await GetByPropertyAsync(p => p.account_id, accountId);
            if (player == null)
            {
                player = new PlayerEntity
                {
                    account_id = accountId,
                    display_name = username
                };
                await InsertAsync(player);
            }

            return player;
        }

    }
}

