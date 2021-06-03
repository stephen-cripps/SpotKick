using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
            readonly Dictionary<string, IEnumerable<string>> trackedArtists = new Dictionary<string, IEnumerable<string>>();
            ISpotifyService spotifyService;

            public Handler(ISongkickService songkickService)
            {
                this.songkickService = songkickService;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var watch = new Stopwatch();
                watch.Start();
                var gigs = await songkickService.FindGigs(request.SongKickUsername);

                spotifyService = new SpotifyService(request.SpotifyAccessToken);

                await CreatePlaylist(gigs
                    .Where(gig => gig.Date < DateTimeOffset.Now.AddDays(30))
                    .Where(gig => gig.Status == Status.Ok)
                    .SelectMany(gig => gig.TrackedArtists)
                    .ToHashSet(), "SpotKick - Next 30 days.");

                await CreatePlaylist(gigs
                    .Where(gig => gig.Attendance != Attendance.NotGoing)
                    .Where(gig => gig.Status != Status.Cancelled)
                    .SelectMany(gig => gig.TrackedArtists)
                    .ToHashSet(), "SpotKick - Interested & Going.");

                await CreatePlaylist(gigs
                    .Where(gig => gig.Attendance == Attendance.Going)
                    .Where(gig => gig.Status != Status.Cancelled)
                    .SelectMany(gig => gig.TrackedArtists)
                    .ToHashSet(), "SpotKick - Just Going.");

                watch.Stop();
                var time = watch.ElapsedMilliseconds;

                return Unit.Value;
            }


            async Task CreatePlaylist(IEnumerable<string> artists, string playlistName)
            {
                var tracks = new List<string>();
                var tasks = artists.Select(artist => Task.Run(async () =>
                {
                    var artistTracks = await GetArtistTracks(artist);
                    lock (tracks)
                        if (artistTracks != null)
                            tracks.AddRange(artistTracks);

                })).ToArray();

                Task.WaitAll(tasks, TimeSpan.FromMinutes(5));

                var playlistId = await spotifyService.GetOrCreatePlaylist(playlistName);

                try
                {
                    await spotifyService.UpdatePlaylist(playlistId, tracks.ToList());
                }
                catch
                {
                    //TODO: Display Error info to user. 
                }
            }

            async Task<IEnumerable<string>> GetArtistTracks(string artistName)
            {
                var artistId = "";
                try
                {
                    lock (trackedArtists)
                        if (trackedArtists.TryGetValue(artistName, out var trackIds))
                            return trackIds;

                    artistId = await spotifyService.FindArtistId(artistName);

                    if (artistId == null)
                        throw new Exception("Artist not found");
                }
                catch (Exception e)
                {
                    UpdateArtistDictionary(artistName, null);
                    return null;

                    //TODO: Display Error info to user. 
                }

                try
                {
                    var tracks = (await spotifyService.GetTopTracks(artistId)).ToList();
                    UpdateArtistDictionary(artistName, null);

                    //TODO: inform user if tracks are null
                    return tracks;
                }
                catch (Exception e)
                {
                    UpdateArtistDictionary(artistName, null);
                    return null;

                    //TODO: Display Error info to user
                }
            }


            void UpdateArtistDictionary(string artistName, IEnumerable<string> tracks)
            {
                lock (trackedArtists)
                    trackedArtists[artistName] = tracks;
            }
        }
    }
}
