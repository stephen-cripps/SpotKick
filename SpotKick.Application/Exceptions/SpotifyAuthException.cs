using System;
using System.Collections.Generic;
using System.Text;

namespace SpotKick.Application.Exceptions
{
    public class SpotifyAuthException : ApplicationException
    {
        public SpotifyAuthException(string message) : base(message)
        {
            
        }
    }
}
