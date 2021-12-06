﻿using Signalling.Models;
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

namespace Signalling.Services
{
    public class SessionQueue : ISessionQueue
    {
        private ConcurrentDictionary<SessionAccession, WebSocket> onlineList;

        private SystemConfig _config;
        
        public SessionQueue(IOptions<SystemConfig> config)
        {
            onlineList = new ConcurrentDictionary<SessionAccession, WebSocket>();
            _config = config.Value;


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
                        Serilog.Log.Information("Fail to ping client");
                        Serilog.Log.Information(ex.Message);
                        Serilog.Log.Information(ex.StackTrace);
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(20).TotalMilliseconds);
            }
        }


        public async Task Handle(SessionAccession accession, WebSocket ws)
        {
            onlineList.TryAdd(accession,ws);
            var core = onlineList.Where(o => o.Key.ID == accession.ID);
            if(core.Count() == 2)
            {
                var sessionCore = core.Where(o => o.Key.Module == Module.CORE_MODULE).First();
                await SendMessage(sessionCore.Value,JsonConvert.SerializeObject(new WebSocketMessage{RequestType = WebSocketMessageResult.REQUEST_STREAM, Content = " "}));
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
                Serilog.Log.Information("Connection closed");
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("Connection closed");
                Serilog.Log.Information(ex.Message);
                Serilog.Log.Information(ex.StackTrace);
            }

            if(accession.Module == Module.CLIENT_MODULE)
            {
                var client = new RestClient();
                var request = new RestRequest(_config.Conductor+"/Sync/Signalling/Disconnect")
                    .AddQueryParameter("WorkerID",accession.WorkerID)
                    .AddQueryParameter("ClientID",accession.ClientID);
                request.Method = Method.Post;

                await client.ExcecuteAsync(request);
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

        public void SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }


        void _handleSdpIceOffer(SessionAccession accession, WebSocketMessage msg, WebSocket ws)
        {
            try
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
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("Fail to handle handshake");                
                Serilog.Log.Information(ex.Message);                
                Serilog.Log.Information(ex.StackTrace);                
            }
        }
    }
}
