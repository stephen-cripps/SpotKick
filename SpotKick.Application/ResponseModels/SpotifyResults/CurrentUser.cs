using Newtonsoft.Json;

namespace SpotKick.Application.ResponseModels.SpotifyResults
{
    public class CurrentUser
    {
        public long Id { get; set; }

        [JsonProperty("display_name")]
        public string Username { get; set; }
    }
}
