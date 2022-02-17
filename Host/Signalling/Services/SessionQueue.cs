using Signalling.Models;
using SharedHost;
using System.Text;
using SharedHost.Models.Session;
using Signalling.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using SharedHost.Models.Device;
using SharedHost.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Signalling.Services
{
    public class SessionQueue : ISessionQueue
    {
        private ConcurrentDictionary<SessionAccession, WebSocket> onlineList;

        private SystemConfig _config;

        private readonly ILog _log;
        
        public SessionQueue(IOptions<SystemConfig> config,
                            ILog log)
        {
            onlineList = new ConcurrentDictionary<SessionAccession, WebSocket>();
            _config = config.Value;
            _log = log;


            Task.Run(() => SystemHeartBeat());
        }

        public async Task SystemHeartBeat()
        {
            while(true)
            {
                foreach(var item in onlineList)
                {
                    try
                    {
                        SendMessage(item.Value,"ping");
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Fail to ping client",ex);
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(20));
            }
        }


        public async Task Handle(SessionAccession accession, WebSocket ws)
        {
            onlineList.AddOrUpdate(accession,ws, (b,c) => ws); 
            var core = onlineList.Where(o => o.Key.ID == accession.ID);
            if(core.Count() == 2)
            {
                var sessionCore = core.Where(o => o.Key.Module == Module.CORE_MODULE).First();
                var initMessage =JsonConvert.SerializeObject(new WebSocketMessage{RequestType = WebSocketMessageResult.REQUEST_STREAM, Content = " "});
                await SendMessage(sessionCore.Value,initMessage);
            }



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
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                _log.Information("Connection closed");
            }
            catch (Exception ex)
            {
                _log.Error("Connection closed",ex);
            }
            onlineList.TryRemove(accession,out var output);
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

        public async Task SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            try
            {
                await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            } catch { _log.Information("Fail to send websocket to client"); }
        }


        async Task _handleSdpIceOffer(SessionAccession accession, WebSocketMessage msg, WebSocket ws)
        {
            try
            {
                foreach(var item in onlineList)
                {
                    if(item.Key.ID == accession.ID)
                    {
                        if(accession.Module == Module.CLIENT_MODULE &&
                           item.Key.Module == Module.CORE_MODULE)
                        {
                            SendMessage(item.Value, JsonConvert.SerializeObject(msg));
                        }
                        if(accession.Module == Module.CORE_MODULE &&
                           item.Key.Module == Module.CLIENT_MODULE)
                        {
                            SendMessage(item.Value, JsonConvert.SerializeObject(msg));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Fail to handle handshake",ex);                
            }
        }
    }
}
