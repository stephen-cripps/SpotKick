using Microsoft.Extensions.DependencyInjection;
using SpotKick.Application;
using System;


namespace SpotKick.ConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {
            //There's probably a better way to do this. 
            var services = Startup.RegisterServices();

            var playlistBuilder = services.GetService<IPlaylistBuilder>();
            playlistBuilder.Create().Wait();

            var scheduler = new Scheduler();

            //Run every day at 17:00. Change this to be set up by GUI once that's built
            scheduler.ScheduleOn(1,17);
        }
    }
}
