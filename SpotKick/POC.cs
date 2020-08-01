using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http.Headers;
using SpotKick.ResponseModels.SpotifyResults;

namespace SpotKick
{
    class POC
    {
        static Settings settings;
        static HttpClient client;
        static string spotifyToken;

        static async Task Main(string[] args)
        {
            settings = GetSettings();
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

        static Settings GetSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json");

            IConfigurationRoot configuration = builder.Build();
            var settings = new Settings();
            configuration.GetSection("Values").Bind(settings);

            return settings;
        }

        static async Task<List<Gig>> FindGigs()
        {
            var entries = new List<CalendarEntry>();
            ResultsPage results;
            var page = 1;
            do
            {
                //TODO: Add Error Handling 
                var uri = $"https://api.songkick.com/api/3.0/users/{settings.SongkickUsername}/calendar.json?reason=tracked_artist&apikey={settings.SongkickApiKey}&page={page}&attendance=all";
                var response = await client.GetAsync(uri);
                results = JsonConvert.DeserializeObject<SongkickResults>(await response.Content.ReadAsStringAsync()).ResultsPage;
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
            var body = new Dictionary<string, string>()
            {
                {"grant_type","refresh_token" },
                { "refresh_token" ,settings.SpotifyRefreshToken}
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://accounts.spotify.com/api/token"),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(body)
            };

            var authString = Base64Encode(settings.SpotifyClientId + ":" + settings.SpotifyClientSecret);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);

            //TODO: Add Error Handling 
            var response = await client.SendAsync(request);

            spotifyToken = JsonConvert.DeserializeObject<SpotifyAuth>(await response.Content.ReadAsStringAsync()).AccessToken;
        }

        static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        static async Task<string> FindArtistId(string name)
        {
            var uri = Uri.EscapeUriString($"https://api.spotify.com/v1/search?q={name}&type=artist");

            //TODO: Add Error Handling 
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

            //TODO: Add Error Handling 
            var response = await client.GetAsync(uri);

            return JsonConvert.DeserializeObject<TopTracks>(await response.Content.ReadAsStringAsync()).Tracks
                .Select(t => t.Id);
        }

        static async Task<string> GetPlaylistId()
        {
            var uri = $"https://api.spotify.com/v1/me/playlists";

            //TODO: Add Error Handling 
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

            //TODO: Add Error Handling 
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

                //TODO: Add Error Handling 
                await client.SendAsync(request);
            }
        }
    }
}
