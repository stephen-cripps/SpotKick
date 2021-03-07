using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace SpotKick.Functions
{
    public class PlaylistController
    {
        readonly IPlaylistBuilder builder;
        readonly IHttpClientFactory test;

        public PlaylistController(IPlaylistBuilder builder, IHttpClientFactory test)
        {
            this.builder = builder;
        }

        [FunctionName("PlaylistCreatorTimer")]
        public async Task
            PlaylistCreatorTimer([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer,ILogger log)
        {
            await builder.Create();
        }

        [FunctionName("PlaylistCreatorHttp")]
        public async Task<IActionResult> PlaylistCreatorHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            await builder.Create();
            return new StatusCodeResult(201); //TODO: Error Handling
        }
    }
}
