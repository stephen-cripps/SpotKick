using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpotKick.Application.Services
{
    public interface ISpotifyService
    {
        Task<string> FindArtistId(string name);

        Task<IEnumerable<string>> GetTopTracks(string id);

        Task<string> GetPlaylistId(string name);

        Task UpdatePlaylist(string id, IReadOnlyCollection<string> tracks);
    }
}
