using System;
using Newtonsoft.Json;

namespace SpotKick.Application.SpotifyAuth
{
    public class SpotifyCredentials
    {
        [JsonProperty("Access_Token")]
        public string AccessToken { get; set; }
        [JsonProperty("Refresh_Token")]
        public string RefreshToken { get; set; }
        [JsonProperty("Token_Type")]
        public string TokenType { get; set; }
        [JsonProperty("Expires_In")]
        public int ExpiresIn { get; set; }
        public DateTime ExpiresOn { get; set; }

        public bool ShouldRefresh => AccessToken != null && DateTime.Now.AddMinutes(59) < ExpiresOn;
    }
}
    