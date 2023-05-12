using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CardTowers_GameServer.Shine.Data
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        Task<TEntity> GetByPropertyAsync<TProperty>(Expression<Func<TEntity, TProperty>> property, TProperty value);
        Task<IEnumerable<TEntity>> GetPaginatedResultAsync(int currentPage, int pageSize = 10);
        Task<int> InsertAsync(TEntity entity);
        Task<bool> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
    }

}
