using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Signalling.Interfaces;
using System.Net;
using SharedHost;
using SystemHub.Interfaces;
using RestSharp;
using SharedHost.Auth;
using Newtonsoft.Json;
using System;

namespace Signalling.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class HubController : ControllerBase
    {
        private readonly IWebSocketHandler _wsHandler;

        private readonly IWebsocketPool _Pool;

        private readonly RestClient _client;

        private readonly SystemConfig _config;

        public HubController(IWebSocketHandler wsHandler, 
                            IWebsocketPool queue,
                            IWebsocketPool pool,
                            SystemConfig config)
        {
            _wsHandler = wsHandler;
            _Pool = queue;
            _config = config;
            _client = new RestClient(config.Authenticator+"/Token");
        }

        [HttpGet("Hub")]
        public async Task<IActionResult> Get(string token)
        {
            var context = ControllerContext.HttpContext;

            if (context.WebSockets.IsWebSocketRequest)
            {
                var tokenRequest = new AuthenticationRequest
                {
                    token = token,
                    Validator = _config.Authenticator
                };

                var request = new RestRequest("Challange")
                    .AddJsonBody(tokenRequest);
                request.Method = Method.POST;

                var result = _client.Execute(request);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var claim = JsonConvert.DeserializeObject<AuthenticationResponse>(result.Content);
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    if (claim.IsUser)
                    {
                        _Pool.AddtoClientHub(Int32.Parse(claim.UserID),webSocket);
                    }
                    if(claim.IsManager)
                    {
                        _Pool.AddtoManagerHub(Int32.Parse(claim.UserID), webSocket);
                    }
                    if(claim.IsAdmin)
                    {
                        _Pool.AddtoAdminHub(webSocket);
                    }

                    await _wsHandler.Handle(webSocket);
                    await _wsHandler.Close(webSocket);
                }
                return new EmptyResult();
            }
            else
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
        }
    }
}
