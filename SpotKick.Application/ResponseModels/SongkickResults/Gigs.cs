using System;

namespace SpotKick.Application.ResponseModels.SongkickResults
{
    public class Gigs
    {
        public ResultsPage ResultsPage { get; set; }
    }

    public class ResultsPage
    {
        public Results Results { get; set; }

        public int PerPage { get; set; }

        public int Page { get; set; }

        public int TotalEntries { get; set; }
    }

    public class Results
    {           
        public CalendarEntry[] CalendarEntry { get; set; }
    }

    public class CalendarEntry
    {
        public Reason Reason { get; set; }

        public Event Event { get; set; }
    }


    public class Reason
    {
        public TrackedArtist[] TrackedArtist { get; set; }

        public string Attendance { get; set; }
    }

    public class TrackedArtist
    {
        public string DisplayName { get; set; }
    }

    public class Event
    {
        public string DisplayName { get; set; }

        public string Status { get; set; }

        public Start Start { get; set; }

        public Location Location { get; set; }

        public Performance[] Performance { get; set; }
    }

    public class Performance
    {
        public string DisplayName { get; set; }
        public string Billing { get; set; }
    }

    public class Start
    {
        public DateTimeOffset Date { get; set; }
    }

    public class Location
    {
        public string City { get; set; }
    }
}