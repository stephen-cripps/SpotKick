namespace SpotKick.Application.ResponseModels.SongkickResults;

public class UserResultsPage
{
    public UserResults Results { get; set; }

    public int PerPage { get; set; }

    public int Page { get; set; }

    public int TotalEntries { get; set; }
}