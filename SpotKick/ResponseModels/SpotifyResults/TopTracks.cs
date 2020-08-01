using System;
using System.Collections.Generic;
using System.Text;

namespace SpotKick.ResponseModels.SpotifyResults
{
    public class TopTracks
    {
        public Track[] Tracks { get; set; }
    }

    public class Track
    {
        public string Id { get; set; }
    }

}
