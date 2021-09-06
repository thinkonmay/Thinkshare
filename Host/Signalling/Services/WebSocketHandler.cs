using Newtonsoft.Json;
using Signalling.Interfaces;
using Signalling.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Signalling.Services
{
    public class WebSocketHandler : IWebSocketHandler
    {
        const int MAX_TIMEOUT_MS = 10000;

        private readonly ISessionQueue Queue;

        public WebSocketHandler(ISessionQueue queue)
        {
            Queue = queue;
        }


        public async Task Handle(WebSocket ws)
        {
            WebSocketReceiveResult message;
            WebSocketMessage WebSocketMessage = new WebSocketMessage();
            do
            {
                using (var memoryStream = new MemoryStream())
                {
                    message = await ReceiveMessage(ws, memoryStream);
                    if (message.Count > 0)
                    {
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

        public void HandleOnlineList(int subjectID, WebSocket ws)
        {
            WebSocketReceiveResult message;
            try
            {
                do
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        message = ReceiveMessage(ws, memoryStream).Result;
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
            } catch (WebSocketException)
            { }
            Queue.DevieGoesOffline(subjectID);
        }

        private async Task<WebSocketReceiveResult> ReceiveMessage(WebSocket ws, Stream memoryStream)
        {
            var readBuffer = new ArraySegment<byte>(new byte[4 * 1024]);
            WebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(readBuffer, CancellationToken.None);
                await memoryStream.WriteAsync(readBuffer.Array, readBuffer.Offset, result.Count,
                    CancellationToken.None);
            } while (!result.EndOfMessage);

            return result;
        }

        public void SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        void _handleClientRequest(WebSocket ws, WebSocketMessage msg)
        {
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
                }
                msg.Result = WebSocketMessageResult.RESULT_ACCEPTED;
                SendMessage(ws, JsonConvert.SerializeObject(msg));

                /*handle in infinite loop*/
                HandleOnlineList(msg.SubjectId, ws);
                return;
            }
            else
            {
                /*if session pair list do not have id of client, send reject message*/
                msg.Result = WebSocketMessageResult.RESULT_REJECTED;
                SendMessage(ws, JsonConvert.SerializeObject(msg));
                return;
            }
        }

        void _handleSlaveRequest(WebSocket ws, WebSocketMessage msg)
        {
            Queue.DeviceGoesOnline(msg.SubjectId, ws);

            if (Queue.SlaveInQueue(msg.SubjectId))
            {
                /*run in infinite loop until slave id is found in onlineList*/
                while (true)
                {
                    if (Queue.ClientIsOnline(msg.SubjectId))
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                /*client id have been found*/
                msg.Result = WebSocketMessageResult.RESULT_ACCEPTED;
                SendMessage(ws, JsonConvert.SerializeObject(msg));

                /*handle in infinite loop*/
                HandleOnlineList(msg.SubjectId, ws);
                return;
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
        }


        public async Task Close(WebSocket ws)
        {
            try
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            } catch (WebSocketException) {  }
            return;
        }
    }
}
