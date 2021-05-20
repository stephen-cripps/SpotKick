using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpotKick.Application.Models;
using SpotKick.Application.ResponseModels.SongkickResults;
using SpotKick.Application.ResponseModels.SpotifyResults;
using SpotKick.Application.Services;

namespace SpotKick.Application
{
    public class PlaylistBuilder : IPlaylistBuilder
    {
        readonly ILogger<PlaylistBuilder> logger;
        readonly ISpotifyService spotifyService;
        readonly ISongkickService songkickService;

        public PlaylistBuilder(ILogger<PlaylistBuilder> logger, ISpotifyService spotifyService, ISongkickService songkickService)
        {
            this.logger = logger;
            this.spotifyService = spotifyService;
            this.songkickService = songkickService;
        }

        public async Task Create(string spotifyAccessToken, string songKickUsername)
        {
            var gigs = await songkickService.FindGigs(songKickUsername);
            logger.LogTrace("Found Gigs");

            throw new NotImplementedException();
            var trackIds = new List<string>();
            var artistsFound = 0;
            var trackedArtists = gigs
                .Where(gig => gig.Date < DateTimeOffset.Now.AddMonths(1))
                .Where(gig => gig.Status != Status.Postponed)
                .SelectMany(gig => gig.TrackedArtists)
                .ToList();

            foreach (var artist in trackedArtists)
            {
                var artistId = "";
                //Need to handle not found artists
                try
                {
                    artistId = await spotifyService.FindArtistId(artist);
                    if (artistId != null)
                        artistsFound++;
                }
                catch (Exception e)
                {
                    logger.LogError("Error finding ID for " + artist);
                    logger.LogError(e.Message);
                    logger.LogError(e.StackTrace);
                }

                try
                {
                    trackIds.AddRange(await spotifyService.GetTopTracks(artistId));
                }
                catch (Exception e)
                {
                    logger.LogError("Error finding tracks for " + artist);
                    logger.LogError(e.Message);
                    logger.LogError(e.StackTrace);
                }
            }

            logger.LogTrace($"Found {artistsFound} of {trackedArtists.Count} artists");

            var playlistId = await spotifyService.GetPlaylistId("Spotkick - Console Test");
            logger.LogTrace("Playlist Id:" + playlistId);

            await spotifyService.UpdatePlaylist(playlistId, trackIds);
            logger.LogTrace("UpdatedPlaylist");
        }


    }
}
