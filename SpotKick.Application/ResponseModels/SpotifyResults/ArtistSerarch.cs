namespace SpotKick.Application.ResponseModels.SpotifyResults
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
