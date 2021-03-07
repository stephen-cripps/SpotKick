using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotKick.Application.ResponseModels.SpotifyResults;

namespace SpotKick.Application
{
    public static class GetSpotifyAccessToken
    {
        public static async Task<string> GetToken(string refreshToken, string clientId, string clientSecret)
        {
            var client = new HttpClient();

            var body = new Dictionary<string, string>()
            {
                {"grant_type","refresh_token" },
                { "refresh_token" ,refreshToken}
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

            return JsonConvert.DeserializeObject<Auth>(await response.Content.ReadAsStringAsync()).AccessToken;
        }

        static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
