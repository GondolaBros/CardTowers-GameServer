using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CardTowers_GameServer.Shine.Data
{
    public static class RepositoryRegistrationExtensions
    {
        public static void AddRepositories(this IServiceCollection services, Assembly assembly)
        {
            var entityTypes = assembly.GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IEntity).IsAssignableFrom(t));

            foreach (var entityType in entityTypes)
            {
                var repositoryType = typeof(BaseRepository<>).MakeGenericType(entityType);
                var repositoryInterfaceType = typeof(IRepository<>).MakeGenericType(entityType);
                services.AddScoped(repositoryInterfaceType, repositoryType);
            }
        }
    }
}

