using RestSharp;
using SharedHost;
using System.Text;
using SharedHost.Models.Session;
using Signalling.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Signalling.Models;
using Newtonsoft.Json;
using SharedHost.Models.Device;

namespace Signalling.Services
{
    public class SessionQueue : ISessionQueue
    {
        private ConcurrentDictionary<SessionAccession, WebSocket> onlineList;
        
        public SessionQueue(SystemConfig config)
        {
            onlineList = new ConcurrentDictionary<SessionAccession, WebSocket>();

            Task.Run(() => SystemHeartBeat());
            Task.Run(() => ConnectionStateCheck());
        }

        public async Task SystemHeartBeat()
        {
            try
            {
                while(true)
                {
                    foreach(var item in onlineList)
                    {
                        var bytes = Encoding.UTF8.GetBytes("ping");
                        var buffer = new ArraySegment<byte>(bytes);
                        await item.Value.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    Thread.Sleep(30000);
                }
            }catch
            {
                await SystemHeartBeat();
            }
        }

        public async Task ConnectionStateCheck()
        {
            try
            {
                foreach (var socket in onlineList)
                {
                    if (socket.Value.State == WebSocketState.Closed)
                    {
                        onlineList.TryRemove(socket);
                    }
                }
                Thread.Sleep(100);
            }
            catch
            {
                await ConnectionStateCheck();
            }
        }


        public async Task Handle(SessionAccession accession, WebSocket ws)
        {
            try
            {
                do
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var message = ReceiveMessage(ws, memoryStream).Result;
                        if (message.Count > 0)
                        {
                            var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                            var WebSocketMessage = JsonConvert.DeserializeObject<WebSocketMessage>(receivedMessage);

                            _handleSdpIceOffer(accession, WebSocketMessage,ws);
                        }
                    }
                } while (ws.State == WebSocketState.Open);
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("Connection closed due to {reason}.", ex.Message);
            }
            // Device goes offline
            await Close(ws);
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


        void _handleSdpIceOffer(SessionAccession accession, WebSocketMessage msg, WebSocket ws)
        {
            foreach(var item in onlineList)
            {
                if(item.Key.ID == accession.ID)
                {
                    if(accession.Module == Module.CLIENT_MODULE)
                    {
                        if(item.Key.WorkerID == accession.WorkerID)
                        {
                            SendMessage(item.Value, JsonConvert.SerializeObject(msg));
                            return;
                        }
                    }
                    else if (accession.Module == Module.CORE_MODULE)
                    {
                        if (item.Key.ClientID == accession.ClientID)
                        {
                            SendMessage(item.Value, JsonConvert.SerializeObject(msg));
                            return;
                        }
                    }
                    else
                    {
                        msg.Result = WebSocketMessageResult.RESULT_REJECTED;
                        SendMessage(ws, JsonConvert.SerializeObject(msg));
                        return;
                    }
                }
            }
            msg.Result = WebSocketMessageResult.RESULT_REJECTED;
            SendMessage(ws, JsonConvert.SerializeObject(msg));
        }


        public async Task Close(WebSocket ws)
        {
            try
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("Connection closed due to {reason}.", ex.Message);
            }
            return;
        }
    }
}
