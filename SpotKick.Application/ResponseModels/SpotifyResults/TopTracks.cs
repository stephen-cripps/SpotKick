namespace SpotKick.Application.ResponseModels.SpotifyResults
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
