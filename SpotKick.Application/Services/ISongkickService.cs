using System.Collections.Generic;
using System.Threading.Tasks;
using SpotKick.Application.Models;

namespace SpotKick.Application.Services
{
    public interface ISongkickService
    {
        /// <summary>
        /// Finds all gig's on a user's calendar. Currently only returns tracked artists. Tied to a user's location.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<List<Gig>> FindGigsFromCalendar(string username);

        /// <summary>
        /// Finds all gigs in the going or attending list for a given user. Returns all artists, Not tied to a user's location.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<List<Gig>> FindGigsFromEvents(string username);

    }
}
