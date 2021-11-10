using Microsoft.Extensions.DependencyInjection;
using ORM.Core;

namespace ORM.Configuration
{
    public class ConsoleConfiguration
    {
        public ServiceProvider Setup()
        {
            ServiceCollection services = new();
            //services.AddSingleton<DbContextOrm>();

            
            return services.BuildServiceProvider();
        }
    }
}