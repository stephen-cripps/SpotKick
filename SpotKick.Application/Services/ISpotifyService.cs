using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotKick.Application.Services
{
    public interface ISpotifyService
    {
        /// <summary>
        /// Searches for an artist, returns their spotify ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<string> FindArtistId(string name);

        /// <summary>
        /// Returns the 10 top tracks for a given artist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetTopTracks(string id);

        /// <summary>
        /// Returns the ID for a playlist with a given name. If the playlist does not exists it is created. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<string> GetOrCreatePlaylist(string name);

        /// <summary>
        /// Replaces the tracks in a playlist
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tracks"></param>
        /// <returns></returns>
        Task UpdatePlaylist(string id, IReadOnlyCollection<string> tracks);

        /// <summary>
        /// Returns the signed in user's username
        /// </summary>
        /// <returns></returns>
        Task<string> GetUsername();
    }
}
