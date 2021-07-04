using System;
using System.Collections.Generic;
using System.Text;

namespace SpotKick.Application.Exceptions
{
    public class ArtistNotFoundException : ApplicationException
    {
        public ArtistNotFoundException(string artistName) : base("Could not find artist: "+ artistName)
        {
            
        }
    }
}
