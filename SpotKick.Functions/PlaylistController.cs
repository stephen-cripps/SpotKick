using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpotKick.Application;

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
            PlaylistCreatorTimer([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer,
                ILogger log)
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
