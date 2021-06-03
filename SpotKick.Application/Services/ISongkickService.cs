using System.Collections.Generic;
using System.Threading.Tasks;
using SpotKick.Application.Models;

namespace SpotKick.Application.Services
{
    public interface ISongkickService
    {
        /// <summary>
        /// Finds all upcoming gigs for a given user. Currently only returns tracked artists.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<List<Gig>> FindGigs(string username);
    }
}
