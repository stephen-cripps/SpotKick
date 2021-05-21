using System;

namespace SpotKick.Application.Exceptions
{
    public class SpotifyAuthException : ApplicationException
    {
        public SpotifyAuthException(string message) : base(message)
        {
            
        }
    }
}
