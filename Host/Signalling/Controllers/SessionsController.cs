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
using SharedHost;
using Newtonsoft.Json;
using SharedHost.Models.Session;
using RestSharp;

namespace Signalling.Controllers
{
    [Route("/Session")]
    [ApiController]
    [Produces("application/json")]
    public class SessionsController : ControllerBase
    {
        private readonly IWebSocketHandler _wsHandler;

        private readonly ISessionQueue Queue;

        public SessionsController(IWebSocketHandler wsHandler, ISessionQueue queue,SystemConfig cofig)
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
                await _wsHandler.Handle(webSocket);
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
