using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SlaveManager.Interfaces;
using Microsoft.AspNetCore.Http;
using SlaveManager.Services;

namespace SlaveManager.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("/[controller]/[action]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentHubHandler AgentHandler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wsHandler"></param>
        public AgentController(IAgentHubHandler wsHandler)
        {
            AgentHandler = wsHandler;
        }

        /// <summary>
        /// entry point for agent to connect with host, upgrade to websocket
        /// </summary>
        /// <returns></returns>
        [HttpGet("/ws")]
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
            else 
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
    }
}
