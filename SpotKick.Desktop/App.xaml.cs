using System;
using System.Configuration;
using System.IO;
using System.Windows;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotKick.Application;
using SpotKick.Application.Services;
using SpotKick.Application.SpotifyAuth;
using SpotKick.Application.UserRepository;

namespace SpotKick.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly ServiceProvider serviceProvider;
        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfiguration configuration = builder.Build();
            
            services
                .AddTransient<ISongkickService, SongkickService>()
                .AddTransient<ISpotifyService, SpotifyService>()
                .AddTransient<ISpotifyAuthService, SpotifyAuthService>()
                .AddTransient<IUserRepo, UserRepo>()
                .AddSingleton<ICurrentUserService, CurrentUserService>()
                .AddLogging()
                .AddMediatR(typeof(CreatePlaylist))
                .AddSingleton<MainWindow>()
                .AddSingleton(configuration);
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                var mainWindow = serviceProvider.GetService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}
