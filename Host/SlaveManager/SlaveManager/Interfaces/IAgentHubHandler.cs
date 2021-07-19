using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using SharedHost.Models;

namespace SlaveManager.Interfaces
{
    public interface IAgentHubHandler
    {
        public Task<Message> ReceiveMessage(WebSocket ws);

        public Task SendMessage(WebSocket ws, string msg);

        public Task Handle(WebSocket ws);
    }
}
