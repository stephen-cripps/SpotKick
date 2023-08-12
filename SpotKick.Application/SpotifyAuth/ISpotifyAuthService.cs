﻿using System.Threading.Tasks;

namespace SpotKick.Application.SpotifyAuth
{
    public interface ISpotifyAuthService
    {
        Task<SpotifyCredentials> GetCredentials();
    }
}