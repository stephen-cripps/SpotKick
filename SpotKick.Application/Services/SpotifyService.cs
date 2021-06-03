using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotKick.Application.ResponseModels.SpotifyResults;

namespace SpotKick.Application.Services
{
    public class SpotifyService : ISpotifyService
    {
        readonly HttpClient spotifyClient;

        public SpotifyService(string authToken)
        {
            spotifyClient = new HttpClient();

            spotifyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            spotifyClient.BaseAddress = new Uri("https://api.github.com/");
        }

        public async Task<string> FindArtistId(string name)
        {
            var uri = Uri.EscapeUriString($"https://api.spotify.com/v1/search?q={name}&type=artist");

            while (true)
            {
                var response = await spotifyClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<ArtistSearch>(content)
                        .Artists
                        .Items
                        .FirstOrDefault()?
                        .Id;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Thread.Sleep(response.Headers.RetryAfter.Delta.Value.Milliseconds);
                }
                else
                    throw new HttpRequestException($"Error Getting Id for {name}: {response.StatusCode}");
            }
        }

        public async Task<IEnumerable<string>> GetTopTracks(string id)
        {
            var uri = $"https://api.spotify.com/v1/artists/{id}/top-tracks?country=from_token";
            while (true)
            {
                var response = await spotifyClient.GetAsync(uri);

                if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<TopTracks>(await response.Content.ReadAsStringAsync()).Tracks.Select(t => t.Id);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Thread.Sleep(response.Headers.RetryAfter.Delta.Value.Milliseconds);
                }
                else
                    throw new HttpRequestException($"Error Getting Tracks for {id}: {response.StatusCode}");
            }
        }

        public async Task<string> GetPlaylistId(string name)
        {
            while (true)
            {
                var response = await spotifyClient.GetAsync("https://api.spotify.com/v1/me/playlists");

                if (response.IsSuccessStatusCode)
                {

                    var playlists = JsonConvert.DeserializeObject<UsersPlaylists>(await response.Content.ReadAsStringAsync())
                        .Playlists;

                    return playlists.Any(p => p.Name == name) ? playlists.SingleOrDefault(p => p.Name == name)?.Id : await BuildPlaylist(name);
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Thread.Sleep(response.Headers.RetryAfter.Delta.Value.Milliseconds);
                }
                else
                    throw new HttpRequestException($"Error Getting playlist id for {name}: {response.StatusCode}");
            }
        }

        async Task<string> BuildPlaylist(string name)
        {
            var userId = (await GetCurrentUser()).Id;

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://api.spotify.com/v1/users/{userId}/playlists"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(new { name = name }))
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            while (true)
            {
                var response = await spotifyClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<ResponseModels.SpotifyResults.CreatePlaylist>(await response.Content.ReadAsStringAsync()).Id;

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Thread.Sleep(response.Headers.RetryAfter.Delta.Value.Milliseconds);
                }
                else
                    throw new HttpRequestException($"Error building playlist for {name}: {response.StatusCode}");
            }
        }

        public async Task UpdatePlaylist(string id, IReadOnlyCollection<string> tracks)
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

                while (true)
                {
                    var response = await spotifyClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                        break;

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        Thread.Sleep(response.Headers.RetryAfter.Delta.Value.Milliseconds);
                    }
                    else
                        throw new HttpRequestException($"Error updating playlist {id}: {response.StatusCode}");
                }
            }
        }

        public async Task<string> GetUsername() => (await GetCurrentUser()).Username;

        async Task<CurrentUser> GetCurrentUser()
        {
            while (true)
            {
                var response = await spotifyClient.GetAsync("https://api.spotify.com/v1/me");

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<CurrentUser>(await response.Content.ReadAsStringAsync());

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Thread.Sleep(response.Headers.RetryAfter.Delta.Value.Milliseconds);
                }
                else
                    throw new HttpRequestException($"Error getting current user: {response.StatusCode}");
            }
        }
    }
}
