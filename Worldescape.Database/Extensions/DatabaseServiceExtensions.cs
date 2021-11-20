using Microsoft.Extensions.DependencyInjection;

namespace Worldescape.Database
{
    public static class DatabaseServiceExtensions
    {
        public static IServiceCollection AddDatabaseService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<DatabaseService>();
            return serviceCollection;
        }
    }
}
