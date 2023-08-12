namespace SpotKick.Application.ResponseModels.SongkickResults;

public class AreaResultsPage
{
    public AreaResults Results { get; set; }

    public int PerPage { get; set; }

    public int Page { get; set; }

    public int TotalEntries { get; set; }
}