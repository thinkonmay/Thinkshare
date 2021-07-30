using Microsoft.AspNetCore.Mvc;
using SlaveManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;








namespace SlaveManager.Controllers
{
    [Route("/Agent")]
    [ApiController]
    public class WebSocketApiController : ControllerBase
    {
        private readonly IWebSocketConnection _connection;

        public WebSocketApiController(IWebSocketConnection connection)
        {
            _connection = connection;
        }

        [HttpGet, ActionName("Register")]
        public async Task<IActionResult> Get()
        {
            var context = ControllerContext.HttpContext;

            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine($"Accepted connection '{context.Connection.Id}'");
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
