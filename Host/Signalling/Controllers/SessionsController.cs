using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Signalling.Interfaces;
using System.Net;
using SharedHost;
using Newtonsoft.Json;
using SharedHost.Models.Session;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Signalling.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionQueue Queue;

        private readonly RestClient Authenticator;

        private readonly SystemConfig  _config;

        public SessionsController(ISessionQueue queue,IOptions<SystemConfig> config)
        {
            Authenticator = new RestClient();
            Queue = queue;
            _config = config.Value;
        }


        [HttpGet("Handshake")]
        public async Task Get(string token)
        {
            var context = ControllerContext.HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {
                var request = new RestRequest(new Uri(_config.SessionTokenValidator))
                    .AddQueryParameter("token", token);
                request.Method = Method.POST;

                var result = await Authenticator.ExecuteAsync(request);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var accession = JsonConvert.DeserializeObject<SessionAccession>(result.Content);
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await Queue.Handle(accession, webSocket);
                }
            }
        }
    }
}
