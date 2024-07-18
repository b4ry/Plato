using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Plato.DatabaseContext;
using System.Windows;

namespace Plato
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            using var serviceScope = ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            
            var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            context!.Database.Migrate();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>();
        }
    }
}
