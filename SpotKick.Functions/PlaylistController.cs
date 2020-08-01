using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpotKick.ResponseModels.SpotifyResults;

namespace SpotKick.Functions
{
    public static class PlaylistController
    {
        static HttpClient client;
        static string spotifyToken;

        [FunctionName("PlaylistCreatorTimer")]
        public static async Task PlaylistCreatorTimer([TimerTrigger("0 0 0 * * 0")]TimerInfo myTimer, ILogger log)
        {
            await Create();
        }

        [FunctionName("PlaylistCreatorHTTP")]
        public static async Task<IActionResult> PlaylistCreatorHTTP(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            await Create();
            return new StatusCodeResult(201); //TODO: Error Handling
        }


        static async Task Create()
        {
            client = new HttpClient();

            var gigs = await FindGigs();

            await GetSpotifyAccessToken();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", spotifyToken);

            var playlistId = await GetPlaylistId();
            var trackIds = new List<string>();

            foreach (var artist in gigs.SelectMany(gig => gig.TrackedArtists))
            {
                var artistId = "";
                //display any not-found artists to user
                try
                {
                    artistId = await FindArtistId(artist);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    trackIds.AddRange(await GetTopTrackIds(artistId));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            await UpdatePlaylist(playlistId, trackIds);
        }

        static async Task<List<Gig>> FindGigs()
        {
            var entries = new List<CalendarEntry>();
            ResultsPage results;
            var page = 1;
            var username = Environment.GetEnvironmentVariable("songkickUsername");
            var apiKey = Environment.GetEnvironmentVariable("songkickApiKey");
            do
            {
                var uri = $"https://api.songkick.com/api/3.0/users/{username}/calendar.json?reason=tracked_artist&apikey={apiKey}&page={page}&attendance=all";
                var response = await client.GetAsync(uri);
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

        static async Task GetSpotifyAccessToken()
        {
            //TODO: Move this stuff to key vault
            var token = Environment.GetEnvironmentVariable("spotifyRefreshToken");
            var clientId = Environment.GetEnvironmentVariable("spotifyClientId");
            var clientSecret = Environment.GetEnvironmentVariable("spotifyClientSecret");

            var body = new Dictionary<string, string>()
            {
                {"grant_type","refresh_token" },
                { "refresh_token" ,token}
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://accounts.spotify.com/api/token"),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(body)
            };

            var authString = Base64Encode(clientId + ":" + clientSecret);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);

            var response = await client.SendAsync(request);

            spotifyToken = JsonConvert.DeserializeObject<Auth>(await response.Content.ReadAsStringAsync()).AccessToken;
        }

        static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        static async Task<string> FindArtistId(string name)
        {
            var uri = Uri.EscapeUriString($"https://api.spotify.com/v1/search?q={name}&type=artist");

            var response = await client.GetAsync(uri);

            return JsonConvert.DeserializeObject<ArtistSearch>(await response.Content.ReadAsStringAsync())
                .Artists
                .Items
                .FirstOrDefault()?
                .Id;
        }

        static async Task<IEnumerable<string>> GetTopTrackIds(string id)
        {
            var uri = $"https://api.spotify.com/v1/artists/{id}/top-tracks?country=from_token";

            var response = await client.GetAsync(uri);

            return JsonConvert.DeserializeObject<TopTracks>(await response.Content.ReadAsStringAsync()).Tracks
                .Select(t => t.Id);
        }

        static async Task<string> GetPlaylistId()
        {
            var uri = $"https://api.spotify.com/v1/me/playlists";

            var response = await client.GetAsync(uri);

            var playlists = JsonConvert.DeserializeObject<UsersPlaylists>(await response.Content.ReadAsStringAsync())
                .Playlists;

            const string name = "SpotKick - Test";

            return playlists.Any(p => p.Name == name) ? playlists.SingleOrDefault(p => p.Name == name)?.Id : await CreatePlaylist(name);
        }

        static async Task<string> CreatePlaylist(string name)
        {
            var userResponse = await client.GetAsync("https://api.spotify.com/v1/me/");
            var userId = JsonConvert.DeserializeObject<CurrentUser>(await userResponse.Content.ReadAsStringAsync()).Id;

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://api.spotify.com/v1/users/{userId}/playlists"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(new { name = name }))
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.SendAsync(request);
            return JsonConvert.DeserializeObject<CreatePlaylist>(await response.Content.ReadAsStringAsync()).Id;
        }

        static async Task UpdatePlaylist(string id, IReadOnlyCollection<string> tracks)
        {
            for (var i = 0; i < tracks.Count; i += 100)
            {
                var items = tracks.Skip(i).Take(100).Select(t => "spotify:track:" + t);

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://api.spotify.com/v1/playlists/{id}/tracks"),
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(items))
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                await client.SendAsync(request);
            }
        }
    }
}
