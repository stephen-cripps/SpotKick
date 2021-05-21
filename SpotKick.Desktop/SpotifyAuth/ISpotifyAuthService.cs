using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotKick.Desktop.SpotifyAuth
{
    public interface ISpotifyAuthService
    {
        Task<SpotifyCredentials> LogIn();

        string RefreshAccessToken(string refreshToken);

        void ForgetUser();
    }
}
