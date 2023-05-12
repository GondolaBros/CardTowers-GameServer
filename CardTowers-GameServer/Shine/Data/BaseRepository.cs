using CardTowers_GameServer.Shine.Data.Entities;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CardTowers_GameServer.Shine.Data
{
    public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly ILogger _logger;

        protected BaseRepository(string connectionString, string tableName, ILogger logger)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _logger = logger;
        }


        public virtual async Task<TEntity> GetByIdAsync(Guid id)
        {
            _logger.LogInformation($"Getting entity with id {id} from table {_tableName}");
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $"SELECT * FROM {_tableName} WHERE id = @Id";
            try
            {
                return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting entity with id {id} from table {_tableName}");
                throw;
            }
        }


        public virtual async Task<IEnumerable<TEntity>> GetPaginatedResultAsync(int currentPage, int pageSize = 10)
        {
            _logger.LogInformation($"Getting paginated result for page {currentPage} and pageSize {pageSize} from table {_tableName}");
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $"SELECT * FROM {_tableName} ORDER BY id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var param = new { Offset = (currentPage - 1) * pageSize, PageSize = pageSize };

            try
            {
                return await connection.QueryAsync<TEntity>(sql, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting paginated result for page {currentPage} and pageSize {pageSize} from table {_tableName}");
                throw;
            }
        }



        public virtual async Task<Guid> InsertAsync(TEntity entity)
        {
            _logger.LogInformation($"Inserting entity into table {_tableName}");
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var columns = GetInsertColumnNames();
            var parameters = GetInsertParameterNames();
            var sql = $@"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING id";

            try
            {
                return await connection.ExecuteScalarAsync<Guid>(sql, entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting entity into table {_tableName}");
                throw;
            }
        }


        public virtual async Task<bool> UpdateAsync(TEntity entity)
        {
            _logger.LogInformation($"Updating entity in table {_tableName}");
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var updateColumns = GetUpdateColumnNames();
            var sql = $@"UPDATE {_tableName} SET {updateColumns} WHERE id = @Id";

            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, entity);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating entity in table {_tableName}");
                throw;
            }
        }


        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation($"Deleting entity with id {id} from table {_tableName}");
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $"DELETE FROM {_tableName} WHERE id = @Id";

            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, new { id = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting entity with id {id} from table {_tableName}");
                throw;
            }
        }


        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            _logger.LogInformation($"Checking if entity with id {id} exists in table {_tableName}");
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $"SELECT COUNT(1) FROM {_tableName} WHERE id = @Id";

            try
            {
                var count = await connection.ExecuteScalarAsync<int>(sql, new { id = id });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if entity with id {id} exists in table {_tableName}");
                throw;
            }
        }


        public virtual async Task<int> CountAsync()
        {
            _logger.LogInformation($"Getting total entity count from table {_tableName}");
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $"SELECT COUNT(*) FROM {_tableName}";

            try
            {
                return await connection.ExecuteScalarAsync<int>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting total entity count from table {_tableName}");
                throw;
            }
        }


        public virtual async Task<TEntity> GetByPropertyAsync<TProperty>(Expression<Func<TEntity, TProperty>> property, TProperty value)
        {
            var propertyName = (property.Body as MemberExpression)?.Member?.Name;
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("Invalid property expression", nameof(property));
            }

            // Ensure the property name only contains letters, numbers, and underscores
            // to help prevent against potentially malicious property name (extremely unlikely)
            if (!Regex.IsMatch(propertyName, @"^[\w]+$"))
            {
                throw new ArgumentException("Invalid characters in property name", nameof(property));
            }

            _logger.LogInformation($"Getting entity with {propertyName} = {value} from table {_tableName}");

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $"SELECT * FROM {_tableName} WHERE {propertyName} = @Value";
            try
            {
                return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Value = value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting entity with {propertyName} = {value} from table {_tableName}");
                throw;
            }
        }


        protected string GetUpdateColumnNames()
        {
            var properties = typeof(TEntity).GetProperties()
                            .Where(p => p.Name != "id" && Regex.IsMatch(p.Name, @"^[\w]+$"));
            return string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
        }


        protected string GetInsertColumnNames()
        {
            var properties = typeof(TEntity).GetProperties()
                            .Where(p => p.Name != "id" && Regex.IsMatch(p.Name, @"^[\w]+$"));
            return string.Join(", ", properties.Select(p => p.Name));
        }

        protected string GetInsertParameterNames()
        {
            var properties = typeof(TEntity).GetProperties()
                            .Where(p => p.Name != "id" && Regex.IsMatch(p.Name, @"^[\w]+$"));
            return string.Join(", ", properties.Select(p => $"@{p.Name}"));
        }
    }
}

