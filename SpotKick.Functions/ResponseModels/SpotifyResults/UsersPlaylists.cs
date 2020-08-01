using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpotKick.ResponseModels.SpotifyResults
{
    public class UsersPlaylists
    {
        [JsonProperty("items")]
        public Playlist[] Playlists { get; set; }

    }

    public class Playlist
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

}
