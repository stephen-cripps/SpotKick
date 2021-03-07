using System;
using System.Collections.Generic;
using SpotKick.Application.ResponseModels.SongkickResults;

namespace SpotKick.Application.Models
{
    public class Gig
    {
        public Gig(IEnumerable<string> trackedArtists, DateTimeOffset date, string location, Status status, string attendance)
        {
            TrackedArtists = trackedArtists;
            Date = date;
            Location = location;
            Status = status;
            this.Attendance = attendance;
        }

        public IEnumerable<string> TrackedArtists { get; }

        public DateTimeOffset Date { get; }

        public string Location { get; }

        public Status Status { get; }

        public string Attendance { get; }
    }
}
