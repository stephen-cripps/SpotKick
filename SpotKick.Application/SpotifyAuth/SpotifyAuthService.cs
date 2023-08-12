using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SpotKick.Application.Exceptions;

namespace SpotKick.Application.SpotifyAuth
{

    /// <summary>
    /// Auth Guide: https://developer.spotify.com/documentation/general/guides/authorization-guide/
    /// Authorization Code Flow with Proof Key for Code Exchange 
    /// </summary>
    public class SpotifyAuthService : ISpotifyAuthService
    {
        private readonly string clientId;
        private readonly string redirectUrl;
        private SpotifyCredentials credentials;
        
        public SpotifyAuthService(IConfiguration configuration)
        {
            clientId = configuration["SpotifyClientId"];
            redirectUrl = configuration["SpotifyRedirectUrl"]; 
        }
        
        public async Task<SpotifyCredentials> GetCredentialsAsync()
        {
            if (credentials == null)
                await LogIn();

            if (!credentials.UserIsValid)
                await RefreshAccessToken(credentials.RefreshToken);
            
            return credentials;
        }

        public SpotifyCredentials GetCredentials()
        {
            return credentials;
        }

        private async Task LogIn()
        {
            var verifier = new CodeVerifier();
            var state = Guid.NewGuid().ToString();

            var url =
                $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUrl}&code_challenge_method=S256&code_challenge={verifier.Challenge}&state={state}&scope=playlist-modify-private,playlist-modify-public";

            var code = await LocalServerCodeReceiver.ReceiveCodeAsync(url, redirectUrl, state);

            credentials = await GetAccessTokenFromCode(code, verifier.Verifier);

            credentials.ExpiresOn = DateTime.Now.AddSeconds(credentials.ExpiresIn);
        }

        private async Task RefreshAccessToken(string refreshToken)
        {
            var client = new HttpClient();

            var body = new Dictionary<string, string>()
                {
                    { "grant_type","refresh_token" },
                    { "refresh_token" ,refreshToken },
                    { "client_id", clientId }
                };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://accounts.spotify.com/api/token"),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(body)
            };

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new SpotifyAuthException("Could not refresh spotify token: " + response.StatusCode);

            credentials = JsonConvert.DeserializeObject<SpotifyCredentials>(await response.Content.ReadAsStringAsync());

            credentials.ExpiresOn = DateTime.Now.AddSeconds(credentials.ExpiresIn);
        }

        private async Task<SpotifyCredentials> GetAccessTokenFromCode(string code, string verifier)
        {
            var client = new HttpClient();

            var body = new Dictionary<string, string>()
            {
                {"client_id",clientId },
                { "grant_type" ,"authorization_code"},
                {"code", code },
                {"redirect_uri", redirectUrl },
                {"code_verifier", verifier }
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://accounts.spotify.com/api/token"),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(body)

            };

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new SpotifyAuthException("Could not get access token from code: " + response.StatusCode);
            }

            return JsonConvert.DeserializeObject<SpotifyCredentials>(await response.Content.ReadAsStringAsync());
        }
    }
}
