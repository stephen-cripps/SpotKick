using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SpotKick.Application.ResponseModels.SongkickResults;
using SpotKick.Application.Services;

namespace SpotKick.Application
{
    public class CreatePlaylist
    {
        public class Command : IRequest
        {
            public string SpotifyAccessToken { get; }
            public string SongKickUsername { get; }

            public Command(string spotifyAccessToken, string songKickUsername)
            {
                SpotifyAccessToken = spotifyAccessToken;
                SongKickUsername = songKickUsername;
            }
        }

        public class handler : IRequestHandler<Command>
        {
            readonly ISongkickService songkickService;

            public handler(ISongkickService songkickService)
            {
                this.songkickService = songkickService;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var gigs = await songkickService.FindGigs(request.SongKickUsername);

                var trackIds = new List<string>();
                var artistsFound = 0;
                var trackedArtists = gigs
                    .Where(gig => gig.Date < DateTimeOffset.Now.AddMonths(1))
                    .Where(gig => gig.Status != Status.Postponed)
                    .SelectMany(gig => gig.TrackedArtists)
                    .ToList();

                var spotifyService = new SpotifyService(request.SpotifyAccessToken);

                foreach (var artist in trackedArtists)
                {
                    var artistId = "";
                    //Need to handle not found artists
                    try
                    {
                        artistId = await spotifyService.FindArtistId(artist);
                        if (artistId != null)
                            artistsFound++;
                        else throw new Exception("Artist not found");
                    }
                    catch (Exception e)
                    {
                        //Display Error info to user. Try Yield Return? Or Just Display in results. 
                        //Maybe try displaying progress in yiled return if possible with mediatr
                        continue;
                    }

                    try
                    {
                        trackIds.AddRange(await spotifyService.GetTopTracks(artistId));
                    }
                    catch (Exception e)
                    {
                        //Display Error info to user
                    }
                }

                var playlistId = await spotifyService.GetPlaylistId("SpotKick - Test");

                await spotifyService.UpdatePlaylist(playlistId, trackIds);

                return Unit.Value;
            }
        }
    }
}
