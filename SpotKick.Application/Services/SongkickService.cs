using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SpotKick.Application.Exceptions;
using SpotKick.Application.Models;
using SpotKick.Application.ResponseModels.SongkickResults;

namespace SpotKick.Application.Services;

public class SongkickService : ISongkickService
{
    private readonly string apiKey;

    public SongkickService(IConfiguration configuration)
    {
        apiKey = configuration["SongkickApiKey"];
    }

    public async Task<List<Gig>> FindUserGigs(string username)
    {
        var client = new HttpClient();
        var entries = new List<CalendarEntry>();
        UserResultsPage userResults;
        var page = 1;

        do
        {
            var uri =
                $"https://api.songkick.com/api/3.0/users/{username}/calendar.json?reason=attendance&apikey={apiKey}&page={page}&attendance=all";
            var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new SongKickUserNotFoundException(username);

                throw new HttpRequestException("Error contacting SongKick: " + response.StatusCode);
            }

            userResults = JsonConvert.DeserializeObject<UserGigs>(await response.Content.ReadAsStringAsync())
                .ResultsPage;
            entries.AddRange(userResults.Results.CalendarEntry);
            page++;
        } while (userResults.Page * userResults.PerPage < userResults.TotalEntries);

        return (from calendarEntry
                in entries
            let displayName = calendarEntry.Event.DisplayName
            let artists = calendarEntry.Event.Performance.Select(p => new Artist()
                {DisplayName = p.DisplayName, Billing = p.Billing})
            let date = calendarEntry.Event.Start.Date
            let location = calendarEntry.Event.Location.City
            let status = calendarEntry.Event.Status
            let attendance = calendarEntry.Reason.Attendance
            select new Gig(artists, date, location, status, attendance, displayName)).ToList();
    }

    public async Task<List<Gig>> FindLocalGigs()
    {
        var metroId = "24475"; // Hardcoding to manchester for now due to laziness
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        var plus7 = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");

        var client = new HttpClient();
        var entries = new List<Event>();
        AreaResultsPage areaResults;
        var page = 1;

        do
        {
            var uri =
                $"https://api.songkick.com/api/3.0/metro_areas/{metroId}/calendar.json?&apikey={apiKey}&min_date={today}&max_date={plus7}&page={page}";
            var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Error contacting SongKick: " + response.StatusCode);
            }

            // Local gigs Just has an event array not calendar entry, so does not deserialse as expected. Need to ungenerisize this
            areaResults = JsonConvert.DeserializeObject<AreaGigs>(await response.Content.ReadAsStringAsync())
                .ResultsPage;
            entries.AddRange(areaResults.Results.Event);
            page++;
        } while (areaResults.Page * areaResults.PerPage < areaResults.TotalEntries);

        return (from ev
                in entries
            let displayName = ev.DisplayName
            let artists = ev.Performance.Select(p => new Artist()
                {DisplayName = p.DisplayName, Billing = p.Billing})
            let date = ev.Start.Date
            let location = ev.Location.City
            let status = ev.Status
            select new Gig(artists, date, location, status, null, displayName)).ToList();
    }
}