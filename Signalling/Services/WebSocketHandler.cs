using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Signalling.Data;
using Signalling.Interfaces;
using Signalling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Signalling.Services
{
    public class WebSocketHandler : IWebSocketHandler
    {
        private readonly ApplicationDbContext _db;

        public WebSocketHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Handle(WebSocket ws)
        {
            while (ws.State == WebSocketState.Open)
            {
                var message = await ReceiveMessage(ws);
                if (message != null)
                {
                    switch (message.RequestType.ToUpper())
                    {
                        case WebSocketMessage.CLIENT_REQUEST:
                            await _handleClientRequest(ws, message);
                            break;
                        case WebSocketMessage.SLAVE_REQUEST:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public async Task<WebSocketMessage> ReceiveMessage(WebSocket ws)
        {
            var buffer = new Memory<byte>();
            var request = await ws.ReceiveAsync(buffer, CancellationToken.None);

            if (request.MessageType == WebSocketMessageType.Text)
            {
                var msg = Encoding.UTF8.GetString(buffer.ToArray());
                return JsonConvert.DeserializeObject<WebSocketMessage>(msg);
            }

            return null;
        }

        public Task SendMessage(WebSocket ws, string msg)
        {
            throw new NotImplementedException();
        }

        async Task _handleClientRequest(WebSocket ws, WebSocketMessage msg)
        {
            var session = await _db.Sessions.FirstOrDefaultAsync(o => o.ClientId == msg.SubjectId);
            if (session != null)
            {

            }
        }
    }
}
