using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
