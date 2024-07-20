using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Plato.DatabaseContext;
using Plato.Encryption;
using System;
using System.Windows;

namespace Plato
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>();
            services.AddSingleton<AesEncryption>();
            services.AddSingleton<MainWindow>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            using var serviceScope = ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            
            var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            context!.Database.Migrate();

            var mainWindow = serviceScope.ServiceProvider.GetService<MainWindow>();
            mainWindow!.Show();
        }
    }
}
