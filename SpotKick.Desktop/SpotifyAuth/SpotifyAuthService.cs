﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotKick.Application.Exceptions;

namespace SpotKick.Desktop.SpotifyAuth
{

    /// <summary>
    /// Auth Guide: https://developer.spotify.com/documentation/general/guides/authorization-guide/
    /// Authorization Code Flow with Proof Key for Code Exchange 
    /// </summary>
    public class SpotifyAuthService : ISpotifyAuthService
    {
        readonly string clientId;
        readonly string redirectUrl;

        public SpotifyAuthService()
        {
            clientId = ConfigurationManager.AppSettings["SpotifyClientId"];
            redirectUrl = ConfigurationManager.AppSettings["SpotifyRedirectUrl"];
        }

        public async Task<SpotifyCredentials> LogIn()
        {
            var verifier = new CodeVerifier();
            var state = Guid.NewGuid().ToString();

            var url =
                $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUrl}&code_challenge_method=S256&code_challenge={verifier.Challenge}&state={state}";

            var code = await LocalServerCodeReceiver.ReceiveCodeAsync(url, redirectUrl, state);

            var credentials = await GetAccessTokenFromCode(code, verifier.Verifier);

            credentials.ExpiresOn = DateTime.Now.AddSeconds(credentials.ExpiresIn);

            return credentials; 
        }

        public string RefreshAccessToken(string refreshToken)
        {
            throw new NotImplementedException();
            //    var client = new HttpClient();

            //    var body = new Dictionary<string, string>()
            //    {
            //        {"grant_type","refresh_token" },
            //        { "refresh_token" ,refreshToken}
            //    };

            //    var request = new HttpRequestMessage()
            //    {
            //        RequestUri = new Uri("https://accounts.spotify.com/api/token"),
            //        Method = HttpMethod.Post,
            //        Content = new FormUrlEncodedContent(body)
            //    };

            //    var authString = Base64Encode(clientId + ":" + clientSecret);
            //    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);

            //    var response = await client.SendAsync(request);

            //    return JsonConvert.DeserializeObject<Auth>(await response.Content.ReadAsStringAsync()).AccessToken;
            //}

            //static string Base64Encode(string plainText)
            //{
            //    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            //    return Convert.ToBase64String(plainTextBytes);
            //}
        }

        public void ForgetUser()
        {
            throw new System.NotImplementedException();
        }

        async Task<SpotifyCredentials> GetAccessTokenFromCode(string code, string verifier)
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
