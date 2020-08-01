using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using Newtonsoft.Json;

namespace SpotKick
{
    public  class SongkickResults
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

    public  class CalendarEntry
    {
        public Reason Reason { get; set; }

        public Event Event { get; set; }
    }

}
