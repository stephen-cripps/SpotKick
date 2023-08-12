namespace SpotKick.Application.ResponseModels.SongkickResults;

public class Event
{
    public string DisplayName { get; set; }

    public string Status { get; set; }

    public Start Start { get; set; }

    public Location Location { get; set; }

    public Performance[] Performance { get; set; }
}