using System.Threading.Tasks;

namespace SpotKick.Application
{
    public interface IPlaylistBuilder
    {
        Task Create(string spotifyAccessToken, string songKickUsername);
    }
}
