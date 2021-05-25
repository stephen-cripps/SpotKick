using System.Threading.Tasks;

namespace SpotKick.Desktop.SpotifyAuth
{
    public interface ISpotifyAuthService
    {
        Task<SpotifyCredentials> LogIn();

        Task<SpotifyCredentials> RefreshAccessToken(string refreshToken);

        void ForgetUser();
    }
}
