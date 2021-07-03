using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SpotKick.Application.Exceptions;
using SpotKick.Application.Models;
using SpotKick.Application.ResponseModels.SongkickResults;
using SpotKick.Application.Services;

namespace SpotKick.Application
{
    /// <summary>
    /// Vertical slice to create the 3 spotkick playlists
    /// </summary>
    public class CreateCSVExport
    {
        public class Command : IRequest
        {
            public string Path { get; }
            public string SongKickUsername { get; }

            public Command(string Path, string songKickUsername)
            {
                this.Path = Path;
                SongKickUsername = songKickUsername;
            }
        }

        public class Handler : IRequestHandler<Command>
        {
            readonly ISongkickService songkickService;
            readonly Dictionary<string, IEnumerable<string>> trackedArtists = new Dictionary<string, IEnumerable<string>>();
            ISpotifyService spotifyService;

            public Handler(ISongkickService songkickService)
            {
                this.songkickService = songkickService;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {

                var gigs = await songkickService.FindGigsFromEvents(request.SongKickUsername);

                string csv = GenerateCSV(gigs);

                try
                {
                    await File.WriteAllTextAsync(request.Path + ".csv", csv);
                }
                catch
                {
                    throw new CsvWriteException();
                }

                return Unit.Value;
            }

            public string GenerateCSV(IEnumerable<Gig> items)
            {
                var output = "";
                var delimiter = ',';

                var properties = typeof(Gig).GetProperties()
                    .Where(p => p.Name == "DisplayName"
                    || p.Name == "Location"
                    || p.Name == "Status");

                using var sw = new StringWriter();

                var header = properties
                .Select(n => n.Name)
                .Aggregate((a, b) => a + delimiter + b);

                sw.WriteLine(header);

                foreach (var item in items)
                {
                    var row = properties
                    .Select(n => n.GetValue(item, null))
                    .Select(n => n == null ? "null" : n.ToString().Replace(',', ';'))
                        .Aggregate((a, b) => a + delimiter + b); sw.WriteLine(row);
                }
                output = sw.ToString();
                return output;
            }
        }
    }
}
