﻿using System;
using System.Configuration;
using System.Windows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SpotKick.Application;
using SpotKick.Application.Services;
using SpotKick.Desktop.SpotifyAuth;
using SpotKick.Desktop.UserRepository;

namespace SpotKick.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        ServiceProvider serviceProvider;
        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        void ConfigureServices(ServiceCollection services)
        {

            services
                .AddTransient<ISongkickService, SongkickService>(x => new SongkickService(ConfigurationManager.AppSettings["SongkickApiKey"]))
                .AddTransient<ISpotifyAuthService, SpotifyAuthService>()
                .AddTransient<IUserRepo, UserRepo>()
                .AddLogging()
                .AddMediatR(typeof(CreatePlaylist))
                .AddSingleton<MainWindow>();
        }

        void OnStartup(object sender, StartupEventArgs e)
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
