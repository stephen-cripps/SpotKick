using System;

namespace SpotKick.Application.Exceptions
{
    public class SongKickUserNotFoundException : ApplicationException
    {
        public SongKickUserNotFoundException(string name) : base("Could not find user: " + name)
        {

        }
    }
}
