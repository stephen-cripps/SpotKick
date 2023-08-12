using SpotKick.Application.SpotifyAuth;

namespace SpotKick.Application.UserRepository
{
    public class UserData
    {
        public UserData()
        {
            SpotifyCredentials = new SpotifyCredentials();
        }

        public SpotifyCredentials SpotifyCredentials { get; set; }      

        public string SongKickUsername { get; set; }

        public string SpotifyUsername { get; set; }
    }
}
