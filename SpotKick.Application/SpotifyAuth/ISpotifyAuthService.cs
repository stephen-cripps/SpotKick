using System.Threading.Tasks;
using SpotKick.Application.ResponseModels.SpotifyResults;

namespace SpotKick.Application.SpotifyAuth;

public interface ISpotifyAuthService
{
    Task<SpotifyCredentials> LogIn();

    Task<SpotifyCredentials> RefreshAccessToken(string refreshToken);

    Task<SpotifyUser> GetCurrentUser(string token);
}