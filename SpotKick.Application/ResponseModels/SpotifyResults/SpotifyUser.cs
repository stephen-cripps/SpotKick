using Newtonsoft.Json;

namespace SpotKick.Application.ResponseModels.SpotifyResults
{
    public class SpotifyUser
    {
        public long Id { get; set; }

        [JsonProperty("display_name")]
        public string Username { get; set; }
    }
}
