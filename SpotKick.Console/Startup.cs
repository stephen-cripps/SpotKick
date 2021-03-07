using System;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotKick.Application;
using SpotKick.Application.Services;

namespace SpotKick.ConsoleApp
{
    public static class Startup
    {
        public static ServiceProvider RegisterServices()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json");

            IConfigurationRoot configuration = builder.Build();
            var settings = new Settings();
            configuration.GetSection("Values").Bind(settings);


            var serviceCollection = new ServiceCollection()
                .AddLogging()
                .AddTransient<IPlaylistBuilder, PlaylistBuilder>()
                .AddTransient<ISpotifyService, SpotifyService>()
                .AddTransient<ISongkickService, SongkickService>(x => new SongkickService(
                    settings.SongkickUsername,
                    settings.SongkickApiKey
                ));

            serviceCollection.AddHttpClient("spotify", client =>
            {
                var refreshToken = settings.SpotifyRefreshToken;
                var clientId = settings.SpotifyClientId;
                var clientSecret = settings.SpotifyClientSecret;

                var test = GetSpotifyAccessToken.GetToken(refreshToken, clientId, clientSecret).Result;

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer",
                        GetSpotifyAccessToken.GetToken(refreshToken, clientId, clientSecret).Result);

                client.BaseAddress = new Uri("https://api.github.com/");
            });

            return serviceCollection.BuildServiceProvider();
        }
    }
}
