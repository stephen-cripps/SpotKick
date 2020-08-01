using System;

namespace SpotKick
{
    public class Event
    {
        public Status Status { get; set; }

        public Start Start { get; set; }

        public Location Location { get; set; }
    }

    public class Start
    {
        public DateTimeOffset Date { get; set; }
    }

    public class Location
    {
        public string City { get; set; }
    }

    public enum Status { Ok, Postponed };

}