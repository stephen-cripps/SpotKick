using System;
using System.Collections.Generic;
using SpotKick.Application.ResponseModels.SongkickResults;

namespace SpotKick.Application.Models
{
    public class Gig
    {
        private readonly Status status;

        public Gig(IEnumerable<Artist> trackedArtists, DateTimeOffset date, string location, string status, string attendance, string displayName)
        {
            Artists = trackedArtists;
            Date = date;
            Location = location;
            DisplayName = displayName;
            this.status = Status;
            Enum.TryParse(status, out this.status);

            Attendance = attendance switch
            {
                "i_might_go" => Attendance.Interested,
                "im_going" => Attendance.Going,
                _ => Attendance.NotGoing
            };
        }
        public string DisplayName { get; }

        public IEnumerable<Artist> Artists { get; }

        public DateTimeOffset Date { get; }

        public string Location { get; }

        public Status Status => status;

        public Attendance Attendance { get; }
    }


    public enum Attendance { NotGoing, Interested, Going }

    public enum Status { Ok, Postponed, Cancelled };
}
