using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotKick.Application.Models;

namespace SpotKick.Application.Services
{
    public interface ISongkickService
    {
        Task<List<Gig>> FindGigs();
    }
}
