using System.IO;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotKick.Application.Services;
using SpotKick.Application.SpotifyAuth;
using SpotKick.Application.UserRepository;

namespace SpotKick.Application;

public static class DependencyInjection
{
    public static void RegisterServices(this ServiceCollection services)
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
            .AddSingleton(configuration);
    }
}