using System;
using System.IO;
using System.Net.Http.Headers;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotKick.Application;
using SpotKick.Application.Services;

namespace SpotKick.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        ServiceProvider serviceProvider;
        IConfigurationRoot config;
        public App()
        {
            ServiceCollection services = new ServiceCollection();
            GetConfig();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void GetConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true);
            config = builder.Build();
        }

        private void ConfigureServices(ServiceCollection services)
        {

            services
                .AddTransient<ISpotifyService, SpotifyService>()
                .AddTransient<ISongkickService, SongkickService>(x => new SongkickService(
                    config["SongkickUsername"],
                    config["SongkickApiKey"]
                ))
                .AddLogging()
                .AddTransient<IPlaylistBuilder, PlaylistBuilder>()
                .AddSingleton<MainWindow>();
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
