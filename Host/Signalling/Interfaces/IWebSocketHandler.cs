using Signalling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Signalling.Interfaces
{
    public interface IWebSocketHandler
    {
<<<<<<< Updated upstream
        Task Handle(WebSocket ws);
        Task<WebSocketMessage> ReceiveMessage(WebSocket ws);
        Task SendMessage(WebSocket ws, string msg);
=======
        public Task Handle(WebSocket ws);
        public void SendMessage(WebSocket ws, string msg);
        public Task Close(WebSocket ws);
>>>>>>> Stashed changes
    }
}
