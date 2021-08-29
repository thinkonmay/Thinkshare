using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlaveManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SlaveManager.Controllers
{
    [Route("/Agent")]
    [ApiController]
    [Produces("application/json")]
    public class WebSocketApiController : ControllerBase
    {
        private readonly IWebSocketConnection _connection;

        static private bool initialized = false;

        public WebSocketApiController(IWebSocketConnection connection, 
                                     ISlavePool slavePool)
        {
            _connection = connection;
        }

        [HttpGet("/Register")]
        public async Task<IActionResult> Get()
        {
            var context = ControllerContext.HttpContext;

            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();    
                await _connection.KeepReceiving(webSocket);
                await _connection.Close(webSocket);
                return new EmptyResult();
            }
            else
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
        }
    }
}
