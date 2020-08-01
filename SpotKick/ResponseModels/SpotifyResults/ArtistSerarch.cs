using System;
using System.Collections.Generic;
using System.Text;

namespace SpotKick.ResponseModels.SpotifyResults
{
    public class ArtistSearch
    {
        public Artists Artists { get; set; }
    }

    public class Artists
    {
        public Artist[] Items { get; set; }
    }

    public class Artist
    {
        public string Id { get; set; }
    }
}
