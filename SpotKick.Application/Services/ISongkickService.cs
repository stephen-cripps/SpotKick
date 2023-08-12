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
        Task<List<Gig>> FindUserGigs(string username);
        
        Task<List<Gig>> FindLocalGigs();
    }
}
