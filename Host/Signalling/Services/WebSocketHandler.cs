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
<<<<<<< Updated upstream
        private readonly ApplicationDbContext _db;
        private ConcurrentDictionary<int, WebSocket> onlineList = new ConcurrentDictionary<int, WebSocket>();

        const int MAX_TIMEOUT_MS = 10000;
        
        public WebSocketHandler(ApplicationDbContext db)
        {
            _db = db;
=======

        const int MAX_TIMEOUT_MS = 10000000;

        private readonly ISessionQueue Queue;

        public WebSocketHandler(ISessionQueue queue)
        {
            Queue = queue;
>>>>>>> Stashed changes
        }

        public async Task Handle(WebSocket ws)
        {
<<<<<<< Updated upstream
            while (ws.State == WebSocketState.Open)
=======
            WebSocketReceiveResult message;
            WebSocketMessage WebSocketMessage = new WebSocketMessage();
            do
>>>>>>> Stashed changes
            {
                var message = await ReceiveMessage(ws);
                if (message != null)
                {
                    switch (message.RequestType.ToUpper())
                    {
<<<<<<< Updated upstream
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
=======
                        var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                        WebSocketMessage = JsonConvert.DeserializeObject<WebSocketMessage>(receivedMessage);
                        break;
                    }
                }
            } while (message.MessageType != WebSocketMessageType.Close && ws.State == WebSocketState.Open);


            switch (WebSocketMessage.RequestType.ToUpper())
            {
                case WebSocketMessageResult.REQUEST_CLIENT:
                    _handleClientRequest(ws, WebSocketMessage); /*handle registration in separate thread*/
                    return;
                case WebSocketMessageResult.REQUEST_SLAVE:
                    _handleSlaveRequest(ws, WebSocketMessage);  /*handle registration in separate thread*/
                    return;
            }
        }

        public void HandleOnlineList(int subjectID,WebSocket ws)
        {
            WebSocketReceiveResult message;
            do
            {
                using (var memoryStream = new MemoryStream())
                {
                    message =  ReceiveMessage(ws, memoryStream).Result;
                    if (message.Count > 0)
                    {
                        var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                        var WebSocketMessage = JsonConvert.DeserializeObject<WebSocketMessage>(receivedMessage);

                        switch (WebSocketMessage.RequestType.ToUpper())
                        {
                            case WebSocketMessageResult.OFFER_SDP:
                                 _handleSdpOffer(ws, WebSocketMessage);
                                break;
                            case WebSocketMessageResult.OFFER_ICE:
                                 _handleIceOffer(ws, WebSocketMessage);
                                break;
                        }
                    }
                }                
            } while (ws.State == WebSocketState.Open);
            Queue.DevieGoesOffline(subjectID);
>>>>>>> Stashed changes
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

        public void SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        void _handleClientRequest(WebSocket ws, WebSocketMessage msg)
        {
<<<<<<< Updated upstream
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
=======
            Queue.DeviceGoesOnline(msg.SubjectId, ws);


            if (Queue.ClientInQueue(msg.SubjectId))
            {
                /*run in infinite loop until slave id is found in onlineList*/
                while (true)
                {
                    if (Queue.SlaveIsOnline(msg.SubjectId))
                    { 
                        break;
                    }
                    Thread.Sleep(100);
>>>>>>> Stashed changes
                }
                msg.Result = WebSocketMessageResult.RESULT_ACCEPTED;
                SendMessage(ws, JsonConvert.SerializeObject(msg));

                /*handle in infinite loop*/
                HandleOnlineList(msg.SubjectId, ws);
                return;
            }
            else
            {
<<<<<<< Updated upstream
                msg.Result = WebSocketMessage.RESULT_TIMEOUT;
                await SendMessage(ws, JsonConvert.SerializeObject(msg));
=======
                /*if session pair list do not have id of client, send reject message*/
                msg.Result = WebSocketMessageResult.RESULT_REJECTED;
                SendMessage(ws, JsonConvert.SerializeObject(msg));
                return;
>>>>>>> Stashed changes
            }
        }

        void _handleSlaveRequest(WebSocket ws, WebSocketMessage msg)
        {
<<<<<<< Updated upstream
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
=======
            Queue.DeviceGoesOnline(msg.SubjectId, ws);

            if (Queue.SlaveInQueue(msg.SubjectId))
            {                    
                /*run in infinite loop until slave id is found in onlineList*/
                while (true)
                {
                    if (Queue.ClientIsOnline(msg.SubjectId)) 
                    {
                        break;
>>>>>>> Stashed changes
                    }
                    Thread.Sleep(100);
                }
<<<<<<< Updated upstream
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
=======
                /*client id have been found*/
                msg.Result = WebSocketMessageResult.RESULT_ACCEPTED;
                SendMessage(ws, JsonConvert.SerializeObject(msg));

                /*handle in infinite loop*/
                HandleOnlineList(msg.SubjectId, ws);
                return;
>>>>>>> Stashed changes
            }
            else
            {
                /*if session pair list do not have id of client, send reject message*/
                msg.Result = WebSocketMessageResult.RESULT_REJECTED;
                SendMessage(ws, JsonConvert.SerializeObject(msg));
                return;
            }            
        }

        void _handleSdpOffer(WebSocket ws, WebSocketMessage msg)
        {
<<<<<<< Updated upstream
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
=======
            WebSocket receiver;

            if (Queue.IsClient(msg.SubjectId))
            {
                receiver = Queue.GetSlaveSocket(msg.SubjectId);
                msg.Result = WebSocketMessageResult.RESULT_ACCEPTED;
                msg.SubjectId = 0;
                SendMessage(receiver, JsonConvert.SerializeObject(msg));
                return;
            }
            else if(Queue.IsSlave(msg.SubjectId))
            {
                receiver = Queue.GetClientSocket(msg.SubjectId);
                msg.Result = WebSocketMessageResult.RESULT_ACCEPTED;
                msg.SubjectId = 0;
                SendMessage(receiver, JsonConvert.SerializeObject(msg));
                return;
            }
        }


        void _handleIceOffer(WebSocket ws, WebSocketMessage msg)
        {
            WebSocket receiver;

            if (Queue.IsClient(msg.SubjectId))
            {
                receiver = Queue.GetSlaveSocket(msg.SubjectId);
                msg.Result = WebSocketMessageResult.RESULT_ACCEPTED;
                msg.SubjectId = 0;
                SendMessage(receiver, JsonConvert.SerializeObject(msg));
                return;
            }
            else if (Queue.IsSlave(msg.SubjectId))
            {
                receiver = Queue.GetClientSocket(msg.SubjectId);
                msg.Result = WebSocketMessageResult.RESULT_ACCEPTED;
                msg.SubjectId = 0;
                SendMessage(receiver, JsonConvert.SerializeObject(msg));
                return;
            }
>>>>>>> Stashed changes
        }

        async Task _handleIce(WebSocket ws, WebSocketMessage msg)
        {
            var session = await _db.Sessions.FirstOrDefaultAsync(o => o.ClientId == msg.SubjectId || o.SlaveId == msg.SubjectId);
            if (session != null)
            {
                WebSocket receiver = null;
                int receiverId = (session.ClientId == msg.SubjectId) ? session.SlaveId : session.ClientId;

<<<<<<< Updated upstream
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
=======
>>>>>>> Stashed changes
    }
}
