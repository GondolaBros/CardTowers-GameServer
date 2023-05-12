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


        public async Task<PlayerEntity> LoadOrCreatePlayerAccount(string accountId)
        {
            var player = await GetByPropertyAsync(p => p.AccountId, accountId);
            if (player == null)
            {
                player = new PlayerEntity { AccountId = accountId };
                await InsertAsync(player);
            }

            return player;
        }

    }
}

