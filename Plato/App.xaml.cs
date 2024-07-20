using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Plato.DatabaseContext;
using Plato.Encryption;
using Plato.ExternalServices;
using System.Configuration;
using System.Windows;

namespace Plato
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>();

            services.AddScoped<AesEncryptor>();
            services.AddSingleton<MainWindow>();

            services.AddHttpClient<IAuthenticationService, AuthenticationService>((client) =>
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings.Get("CerberusApiUrl")!);
            });
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
