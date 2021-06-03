using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SpotKick.Application.Services;

namespace SpotKick.Application
{
    public class ReadSpotifyUsername
    {
        public class Query : IRequest<string>
        {
            public string SpotifyAccessToken { get; }

            public Query(string spotifyAccessToken)
            {
                SpotifyAccessToken = spotifyAccessToken;
            }
        }

        public class Handler : IRequestHandler<Query, string>
        {
            public async Task<string> Handle(Query request, CancellationToken cancellationToken)
            {
                var spotifyService = new SpotifyService(request.SpotifyAccessToken);

                return await spotifyService.GetUsername(); 
            }
        }
    }
}
