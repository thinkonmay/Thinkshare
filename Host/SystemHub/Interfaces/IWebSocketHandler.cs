using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Signalling.Interfaces
{
    public interface IWebSocketHandler
    {
        public Task Handle(WebSocket ws);
        public void SendMessage(WebSocket ws, string msg);
        public Task Close(WebSocket ws);
    }
}
