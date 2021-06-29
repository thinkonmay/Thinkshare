using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Signalling.Data;
using Signalling.Interfaces;
using Signalling.Models;
using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<int, WebSocket> onlineList = new ConcurrentDictionary<int, WebSocket>();

        const int MAX_TIMEOUT_MS = 10000;
        
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
                        case WebSocketMessage.REQUEST_CLIENT:
                            await _handleClientRequest(ws, message);
                            break;
                        case WebSocketMessage.REQUEST_SLAVE:
                            await _handleSlaveRequest(ws, message);
                            break;
                        case WebSocketMessage.OFFER_SDP:
                            await _handleSdpOffer(ws, message);
                            break;
                        case WebSocketMessage.ANSWER_SDP:
                            await _handleSdpAnswer(ws, message);
                            break;
                        case WebSocketMessage.OFFER_ICE:
                            await _handleIce(ws, message);
                            break;
                        default:
                            break;
                    }
                }

                if (ws.State == WebSocketState.CloseSent || ws.State == WebSocketState.Closed)
                {
                    onlineList.TryRemove(message.SubjectId, out ws);
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

        public async Task SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        async Task _handleClientRequest(WebSocket ws, WebSocketMessage msg)
        {
            Task t = Task.Run(async () =>
            {
                var session = await _db.Sessions.FirstOrDefaultAsync(s => s.ClientId == msg.SubjectId);
                if (session != null)
                {
                    if (!onlineList.ContainsKey(session.ClientId))
                    {
                        onlineList.TryAdd(session.ClientId, ws);
                    }

                    while (true)
                    {
                        if (!onlineList.ContainsKey(session.SlaveId)) continue;

                        msg.Result = WebSocketMessage.RESULT_ACCEPTED;
                        await SendMessage(ws, JsonConvert.SerializeObject(msg));
                    }
                }
                else
                {
                    msg.Result = WebSocketMessage.RESULT_REJECTED;
                    await SendMessage(ws, JsonConvert.SerializeObject(msg));
                    return;
                }
            });

            if (!t.Wait(MAX_TIMEOUT_MS))
            {
                msg.Result = WebSocketMessage.RESULT_TIMEOUT;
                await SendMessage(ws, JsonConvert.SerializeObject(msg));
            }
        }

        async Task _handleSlaveRequest(WebSocket ws, WebSocketMessage msg)
        {
            Task t = Task.Run(async () =>
            {
                var session = await _db.Sessions.FirstOrDefaultAsync(s => s.SlaveId == msg.SubjectId);
                if (session != null)
                {
                    if (!onlineList.ContainsKey(session.SlaveId))
                    {
                        onlineList.TryAdd(session.SlaveId, ws);
                    }

                    while (true)
                    {
                        if (!onlineList.ContainsKey(session.ClientId)) continue;

                        msg.Result = WebSocketMessage.RESULT_ACCEPTED;
                        await SendMessage(ws, JsonConvert.SerializeObject(msg));
                    }
                }
                else
                {
                    msg.Result = WebSocketMessage.RESULT_REJECTED;
                    await SendMessage(ws, JsonConvert.SerializeObject(msg));
                    return;
                }
            });

            if (!t.Wait(MAX_TIMEOUT_MS))
            {
                msg.Result = WebSocketMessage.RESULT_TIMEOUT;
                await SendMessage(ws, JsonConvert.SerializeObject(msg));
            }
        }

        async Task _handleSdpOffer(WebSocket ws, WebSocketMessage msg)
        {
            var session = await _db.Sessions.FirstOrDefaultAsync(o => o.ClientId == msg.SubjectId);
            WebSocket receiver;

            if (session != null && onlineList.TryGetValue(session.SlaveId, out receiver))
            {
                msg.Result = WebSocketMessage.RESULT_ACCEPTED;
                await SendMessage(receiver, msg.Content);
                return;
            }

            msg.Result = WebSocketMessage.RESULT_REJECTED;
            await SendMessage(ws, JsonConvert.SerializeObject(msg));
        }

        async Task _handleSdpAnswer(WebSocket ws, WebSocketMessage msg)
        {
            var session = await _db.Sessions.FirstOrDefaultAsync(o => o.SlaveId == msg.SubjectId);
            WebSocket receiver;

            if (session != null && onlineList.TryGetValue(session.ClientId, out receiver))
            {
                msg.Result = WebSocketMessage.RESULT_ACCEPTED;
                await SendMessage(receiver, msg.Content);
                return;
            }

            msg.Result = WebSocketMessage.RESULT_REJECTED;
            await SendMessage(ws, JsonConvert.SerializeObject(msg));
        }

        async Task _handleIce(WebSocket ws, WebSocketMessage msg)
        {
            var session = await _db.Sessions.FirstOrDefaultAsync(o => o.ClientId == msg.SubjectId || o.SlaveId == msg.SubjectId);
            if (session != null)
            {
                WebSocket receiver = null;
                int receiverId = (session.ClientId == msg.SubjectId) ? session.SlaveId : session.ClientId;

                if (onlineList.TryGetValue(receiverId, out receiver))
                {
                    msg.SubjectId = receiverId;
                    await SendMessage(receiver, JsonConvert.SerializeObject(msg));
                }
                else
                {
                    msg.Result = WebSocketMessage.RESULT_REJECTED;
                    await SendMessage(ws, JsonConvert.SerializeObject(msg));
                }
            }
        }
    }
}
