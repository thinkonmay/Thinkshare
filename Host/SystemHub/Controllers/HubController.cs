using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using SharedHost;
using SystemHub.Interfaces;
using RestSharp;
using SharedHost.Auth;
using Newtonsoft.Json;

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
            _Pool = queue;
            _config = config;
            _wsHandler = wsHandler;
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
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var claim = JsonConvert.DeserializeObject<AuthenticationResponse>(result.Content);
                    if(!claim.IsUser && !claim.IsManager & !claim.IsAdmin)
                    {
                        return NotFound();
                    }
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    _Pool.AddtoPool(claim, webSocket);
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
