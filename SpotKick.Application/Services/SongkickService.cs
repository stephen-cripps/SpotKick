using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotKick.Application.Models;
using SpotKick.Application.ResponseModels.SongkickResults;

namespace SpotKick.Application.Services
{
    public class SongkickService : ISongkickService
    {
        readonly string username;
        readonly string apiKey;

        public SongkickService(string username, string apiKey)
        {
            this.username = username;
            this.apiKey = apiKey;
        }

        public async Task<List<Gig>> FindGigs()
        {
            var client = new HttpClient();
            var entries = new List<CalendarEntry>();
            ResultsPage results;
            var page = 1;

            do
            {
                var uri = $"https://api.songkick.com/api/3.0/users/{username}/calendar.json?reason=tracked_artist&apikey={apiKey}&page={page}&attendance=all";
                var response = await client.GetAsync(uri);
                results = JsonConvert.DeserializeObject<Gigs>(await response.Content.ReadAsStringAsync()).ResultsPage;
                entries.AddRange(results.Results.CalendarEntry);
                page++;
            } while (results.Page * results.PerPage < results.TotalEntries);

            //TODO: Add Automapper
            return (from calendarEntry
                    in entries
                let artists = calendarEntry.Reason.TrackedArtist.Select(a => a.DisplayName)
                let date = calendarEntry.Event.Start.Date
                let location = calendarEntry.Event.Location.City
                let status = calendarEntry.Event.Status
                let attendance = calendarEntry.Reason.Attendance
                select new Gig(artists, date, location, status, attendance)).ToList();
        }
    }
}
