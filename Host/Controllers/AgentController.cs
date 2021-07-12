using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Host.Data;
using Host.Interfaces;
using Host.Models;

namespace Host.Controllers
{
    public class AgentController : Controller
    {
        private readonly IAgentHub _wsHandler;

        public AgentController(IAgentHub wsHandler)
        {
            _wsHandler = wsHandler;
        }

        public async Task Pair()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    await _wsHandler.Handle(webSocket);
                }
            }
        }
    }
}
