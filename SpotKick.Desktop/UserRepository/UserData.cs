﻿using SpotKick.Desktop.SpotifyAuth;

namespace SpotKick.Desktop.UserRepository
{
    public class UserData
    {
        public SpotifyCredentials SpotifyCredentials { get; set; }      

        public string SongKickUsername { get; set; }

        public string ButtonText => SpotifyCredentials.UserIsValid ? "Update Playlists" : "Spotify Login";
    }
}