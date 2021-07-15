using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SharedHost.Interfaces;
using SharedHost.Models;
using Microsoft.AspNetCore.Http;
using SharedHost.Services;

namespace SharedHost.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("/[controller]/[action]")]
    public class AgentController : ControllerBase
    {
        private readonly AgentHubHandler AgentHandler;

        public AgentController(AgentHubHandler wsHandler)
        {
            AgentHandler = wsHandler;
        }

        /// <summary>
        /// entry point for agent to connect with host, upgrade to websocket
        /// </summary>
        /// <returns></returns>
        [HttpGet, ActionName ("Register")]
        public async Task Register()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    _ = Task.Run(()=> AgentHandler.Handle(webSocket));
                }
            }
        }
    }
}
