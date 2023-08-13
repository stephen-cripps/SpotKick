using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotKick.Application;
using SpotKick.Application.Services;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.RegisterServices();

using IHost host = builder.Build();

var currentUserService = host.Services.GetService<ICurrentUserService>();
var mediator = host.Services.GetService<IMediator>();

var user = await currentUserService.ValidateAndGetCurrentUserAsync();

if (string.IsNullOrWhiteSpace(user.SongKickUsername))
{
    Console.Write("Please enter your Songkick username: ");
    user.SongKickUsername = Console.ReadLine();
    currentUserService.StoreSongkickUsername(user.SongKickUsername);
}

var command = new CreatePlaylist.Command(user.SongKickUsername);
await mediator.Send(command);