using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public class Handler : IRequestHandler<Command>
        {
            readonly ISongkickService songkickService;
            ISpotifyService spotifyService;
            Dictionary<string, IEnumerable<string>> topTrackCache = new Dictionary<string, IEnumerable<string>>();
            List<Gig> upcomingGigs;
            List<Gig> attendingGigs;

            public Handler(ISongkickService songkickService)
            {
                this.songkickService = songkickService;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                upcomingGigs = await songkickService.FindGigs(request.SongKickUsername, Services.Reason.tracked_artist);
                attendingGigs = await songkickService.FindGigs(request.SongKickUsername, Services.Reason.attendance);

                spotifyService = new SpotifyService(request.SpotifyAccessToken);

                await CreatePlaylist(upcomingGigs
                    .Where(gig => gig.Date < DateTimeOffset.Now.AddDays(30))
                    .Where(gig => gig.Status == Status.Ok)
                    .SelectMany(gig => gig.Artists)
                    .ToHashSet(), "SpotKick - Next 30 days.");

                await CreatePlaylist(attendingGigs
                    .Where(gig => gig.Status != Status.Cancelled)
                    .SelectMany(gig => gig.Artists)
                    .ToHashSet(), "SpotKick - Interested & Going.");

                await CreatePlaylist(attendingGigs
                    .Where(gig => gig.Attendance == Attendance.Going)
                    .Where(gig => gig.Status != Status.Cancelled)
                    .SelectMany(gig => gig.Artists)
                    .ToHashSet(), "SpotKick - Just Going.");

                return Unit.Value;
            }


            async Task CreatePlaylist(IEnumerable<Artist> artists, string playlistName)
            {
                var tracks = new List<string>();
                foreach (var artist in artists)
                {
                    tracks.AddRange(await GetArtistTracks(artist));
                }

                var playlistId = await spotifyService.GetOrCreatePlaylist(playlistName);

                try
                {
                    await spotifyService.UpdatePlaylist(playlistId, tracks.ToList());
                }
                catch(Exception e)
                {
                    //TODO: Display Error info to user. 
                }
            }

            async Task<IEnumerable<string>> GetArtistTracks(Artist artist)
            {
                var tracks = new List<string>();
                try
                {
                    if (topTrackCache.TryGetValue(artist.DisplayName, out var trackIds))
                        return trackIds;

                    var artistId = await spotifyService.FindArtistId(artist.DisplayName);

                    if (artistId != null)
                        tracks = (await spotifyService.GetTopTracks(artistId)).ToList();
                    else
                        throw new ArtistNotFoundException(artist.DisplayName);
                }
                catch (Exception e)
                {
                    //TODO: Inform user if artist wasn't found tracks are null or there has been an error
                }

                topTrackCache[artist.DisplayName] = tracks;
                return tracks;
            }
        }
    }
}
