using System.Collections.Generic;

namespace SpotKick.Application.ResponseModels.SongkickResults;

public class AreaResults
{
    public IEnumerable<Event> Event { get; set; }
}