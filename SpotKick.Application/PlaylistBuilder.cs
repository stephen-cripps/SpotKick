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

namespace SpotKick.Application
{
    public class PlaylistBuilder : IPlaylistBuilder
    {
        readonly HttpClient spotifyClient;
        readonly ILogger<PlaylistBuilder> logger;

        public PlaylistBuilder(ILogger<PlaylistBuilder> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            spotifyClient = httpClientFactory.CreateClient("spotify");
        }

        public async Task Create()
        {
            var gigs = await FindGigs();
            logger.LogTrace("Found Gigs");

            var playlistId = await GetPlaylistId();
            logger.LogTrace("Playlist Id:" + playlistId);

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
                    artistId = await FindArtistId(artist);
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
                    trackIds.AddRange(await GetTopTrackIds(artistId));
                }
                catch (Exception e)
                {
                    logger.LogError("Error finding tracks for " + artist);
                    logger.LogError(e.Message);
                    logger.LogError(e.StackTrace);
                }
            }

            logger.LogTrace($"Found {artistsFound} of {trackedArtists.Count} artists");

            await UpdatePlaylist(playlistId, trackIds);

            logger.LogTrace("UpdatedPlaylist");
        }

        static async Task<List<Gig>> FindGigs()
        {
            var client = new HttpClient();
            var entries = new List<CalendarEntry>();
            ResultsPage results;
            var page = 1;
            var username = Environment.GetEnvironmentVariable("songkickUsername");
            var apiKey = Environment.GetEnvironmentVariable("songkickApiKey");
            do
            {
                var uri = $"https://api.songkick.com/api/3.0/users/{username}/calendar.json?reason=tracked_artist&apikey={apiKey}&page={page}&attendance=all";
                var response = await client.GetAsync(uri);
                var test = await response.Content.ReadAsStringAsync();
                results = JsonConvert.DeserializeObject<Gigs>(await response.Content.ReadAsStringAsync()).ResultsPage;
                entries.AddRange(results.Results.CalendarEntry);
                page++;
            } while (results.Page * results.PerPage < results.TotalEntries);

            //TODO: Add Automapper
            return (from calendarEntry
                in entries
                    let artists = calendarEntry.Reason.TrackedArtist.Select(a => a.DisplayName)
                    let date = calendarEntry.Event.Start.Date
                    let location = calendarEntry.Event.Location.City
                    let status = calendarEntry.Event.Status
                    let attendance = calendarEntry.Reason.Attendance
                    select new Gig(artists, date, location, status, attendance)).ToList();
        }

        async Task<string> FindArtistId(string name)
        {
            var uri = Uri.EscapeUriString($"https://api.spotify.com/v1/search?q={name}&type=artist");

            var response = await spotifyClient.GetAsync(uri);

            var content = await response.Content.ReadAsStringAsync();


            //Why is content null sometimes
            try
            {
                return JsonConvert.DeserializeObject<ArtistSearch>(content)
                    .Artists
                    .Items
                    .FirstOrDefault()?
                    .Id;
            }
            catch
            {
                logger.LogError("Failed FindArtistId Content: " + content);
                throw;
            }
        }

        async Task<IEnumerable<string>> GetTopTrackIds(string id)
        {
            var uri = $"https://api.spotify.com/v1/artists/{id}/top-tracks?country=from_token";

            var response = await spotifyClient.GetAsync(uri);

            return JsonConvert.DeserializeObject<TopTracks>(await response.Content.ReadAsStringAsync()).Tracks
                .Select(t => t.Id);
        }

        async Task<string> GetPlaylistId()
        {
            var uri = "https://api.spotify.com/v1/me/playlists";

            var response = await spotifyClient.GetAsync(uri);

            var playlists = JsonConvert.DeserializeObject<UsersPlaylists>(await response.Content.ReadAsStringAsync())
                .Playlists;

            const string name = "SpotKick - Another Test";

            return playlists.Any(p => p.Name == name) ? playlists.SingleOrDefault(p => p.Name == name)?.Id : await BuildPlaylist(name);
        }

        async Task<string> BuildPlaylist(string name)
        {
            var userResponse = await spotifyClient.GetAsync("https://api.spotify.com/v1/me/");
            var userId = JsonConvert.DeserializeObject<CurrentUser>(await userResponse.Content.ReadAsStringAsync()).Id;

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://api.spotify.com/v1/users/{userId}/playlists"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(new { name = name }))
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await spotifyClient.SendAsync(request);
            return JsonConvert.DeserializeObject<ResponseModels.SpotifyResults.CreatePlaylist>(await response.Content.ReadAsStringAsync()).Id;
        }

        async Task UpdatePlaylist(string id, IReadOnlyCollection<string> tracks)
        {
            for (var i = 0; i < tracks.Count; i += 100)
            {
                var items = tracks.Skip(i).Take(100).Select(t => "spotify:track:" + t);

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://api.spotify.com/v1/playlists/{id}/tracks"),
                    Method = HttpMethod.Put,
                    Content = new StringContent(JsonConvert.SerializeObject(new { uris = items }))
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                await spotifyClient.SendAsync(request);
            }
        }
    }
}
