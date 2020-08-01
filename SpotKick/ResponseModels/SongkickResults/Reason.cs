namespace SpotKick
{
    public class Reason
    {
        public TrackedArtist[] TrackedArtist { get; set; }

        public string Attendance { get; set; }
    }

    public class TrackedArtist
    {
        public string DisplayName { get; set; }
    }
}