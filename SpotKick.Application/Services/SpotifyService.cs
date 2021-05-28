using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

            var response = await spotifyClient.GetAsync(uri);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ArtistSearch>(content)
                .Artists
                .Items
                .FirstOrDefault()?
                .Id;
        }

        public async Task<IEnumerable<string>> GetTopTracks(string id)
        {
            var uri = $"https://api.spotify.com/v1/artists/{id}/top-tracks?country=from_token";

            var response = await spotifyClient.GetAsync(uri);

            return JsonConvert.DeserializeObject<TopTracks>(await response.Content.ReadAsStringAsync()).Tracks
                .Select(t => t.Id);
        }

        public async Task<string> GetPlaylistId(string name)
        {
            var response = await spotifyClient.GetAsync("https://api.spotify.com/v1/me/playlists");

            var playlists = JsonConvert.DeserializeObject<UsersPlaylists>(await response.Content.ReadAsStringAsync())
                .Playlists;

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

                await spotifyClient.SendAsync(request);
            }
        }

        public async Task<string> GetUsername()
        {
            var uri = $"https://api.spotify.com/v1/me";

            var response = await spotifyClient.GetAsync(uri);

           return JsonConvert.DeserializeObject<CurrentUser>(await response.Content.ReadAsStringAsync()).Username;
        }
    }
}
