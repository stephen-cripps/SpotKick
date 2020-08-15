using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace SpotKick.Application
{
    public interface IPlaylistBuilder
    {
        Task Create();
    }
}
