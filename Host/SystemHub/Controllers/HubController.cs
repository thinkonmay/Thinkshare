using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using SharedHost;
using SystemHub.Interfaces;
using RestSharp;
using System;
using SharedHost.Auth;
using Newtonsoft.Json;
using SharedHost.Models.Cluster;
using Microsoft.Extensions.Options;

namespace SystemHub.Controllers
{
    [ApiController]
    [Route("/Hub")]
    [Produces("application/json")]
    public class HubController : ControllerBase
    {

        private readonly IUserSocketPool _User;
        private readonly IClusterSocketPool _Cluster;
        private readonly RestClient _TokenValidator;
        private readonly SystemConfig _config;

        public HubController(IClusterSocketPool cluster,
                            IUserSocketPool user,
                            IOptions<SystemConfig> config)
        {
            _User = user;
            _Cluster = cluster;
            _config = config.Value;
            _TokenValidator = new RestClient();
        }

        [HttpGet("User")]
        public async Task GetUser(string token)
        {
            var context = ControllerContext.HttpContext;

            if (context.WebSockets.IsWebSocketRequest)
            {
                var tokenRequest = new AuthenticationRequest
                {
                    token = token,
                    Validator = "authenticator"
                };

                var request = new RestRequest(new Uri(_config.Authenticator+"/Token/Challenge/User"))
                    .AddJsonBody(tokenRequest);
                request.Method = Method.POST;

                var result = await _TokenValidator.ExecuteAsync(request);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var claim = JsonConvert.DeserializeObject<AuthenticationResponse>(result.Content);
                    if(!claim.IsUser && !claim.IsManager & !claim.IsAdmin)
                    {
                        return;
                    }
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    await _User.AddtoPool(claim, webSocket);
                }
            }
        }

        [HttpGet("Cluster")]
        public async Task GetWorker(string token)
        {
            var context = ControllerContext.HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {
                var request = new RestRequest(_config.Authenticator+"/Token/Challenge/Cluster")
                    .AddQueryParameter("token",token);
                request.Method = Method.POST;

                var result = await _TokenValidator.ExecuteAsync(request);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var claim = JsonConvert.DeserializeObject<ClusterCredential>(result.Content);
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    await _Cluster.AddtoPool(claim, webSocket);
                }
            }
        }
    }
}
