using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Signalling.Models;
using Signalling.Interfaces;
using System.Net;

namespace Signalling.Controllers
{
    [Route("/Session")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly IWebSocketHandler _wsHandler;

        private readonly ISessionQueue Queue;

        public SessionsController(IWebSocketHandler wsHandler, ISessionQueue queue)
        {
            _wsHandler = wsHandler;
            Queue = queue;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var context = ControllerContext.HttpContext;

            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Task t = _wsHandler.Handle(webSocket);

                t.Wait();

                await _wsHandler.Close(webSocket);

                return new EmptyResult();
            }
            else
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
        }
    }
}
