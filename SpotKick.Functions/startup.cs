using System;
using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using SpotKick.Functions;
using Microsoft.Extensions.DependencyInjection;
using SpotKick.Application;
using SpotKick.Application.Services;


[assembly: FunctionsStartup(typeof(Startup))]
namespace SpotKick.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddHttpClient("spotify", client =>
            {
                var refreshToken = Environment.GetEnvironmentVariable("spotifyRefreshToken");
                var clientId = Environment.GetEnvironmentVariable("spotifyClientId");
                var clientSecret = Environment.GetEnvironmentVariable("spotifyClientSecret");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer",
                        GetSpotifyAccessToken.GetToken(refreshToken, clientId, clientSecret).Result);

                client.BaseAddress = new Uri("https://api.github.com/");
            });

            builder.Services.AddTransient<IPlaylistBuilder, PlaylistBuilder>()
                .AddTransient<ISpotifyService, SpotifyService>()
                .AddTransient<ISongkickService, SongkickService>(x => new SongkickService(
                    Environment.GetEnvironmentVariable("songkickUsername"),
                    Environment.GetEnvironmentVariable("songkickApiKey")
                ));
        }
    }
}