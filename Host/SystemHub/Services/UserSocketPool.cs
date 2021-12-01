using SharedHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using SystemHub.Interfaces;
using System.Threading.Tasks;
using SharedHost.Auth;
using SharedHost.Models.Hub;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Options;

namespace SystemHub.Services
{
    public class UserSocketPool : IUserSocketPool
    {
        private readonly List<KeyValuePair<AuthenticationResponse, WebSocket>> _UserSocketsPool;        
        
        public UserSocketPool(IOptions<SystemConfig> config)
        {
            _UserSocketsPool =    new List<KeyValuePair<AuthenticationResponse, WebSocket>>();

            Task.Run(() => ConnectionHeartBeat());
            Task.Run(() => ConnectionStateCheck());
        }



        public async Task ConnectionHeartBeat()
        {
            try
            {                
                while (true)
                {
                    foreach (var socket in _UserSocketsPool)
                    {
                        if(socket.Value.State == WebSocketState.Open)
                        {
                            await SendMessage(socket.Value,"ping");
                        }
                    }
                    Thread.Sleep(30*1000);
                }
            }catch
            {
                await ConnectionHeartBeat();
            }
        }

        public async Task ConnectionStateCheck()
        {
            try
            {                
                while (true)
                {
                    foreach (var socket in _UserSocketsPool)
                    {
                        if(socket.Value.State == WebSocketState.Closed) 
                        {
                            _UserSocketsPool.Remove(socket);
                        }
                    }
                    Thread.Sleep(100);
                }
            }catch
            {
                await ConnectionStateCheck();
            }
        }


        public async Task AddtoPool(AuthenticationResponse resp,WebSocket session)
        {
            _UserSocketsPool.Add(new KeyValuePair<AuthenticationResponse,WebSocket>(resp,session));
            await Handle(session);
            _UserSocketsPool.Remove(new KeyValuePair<AuthenticationResponse,WebSocket>(resp, session));
        }




        public async Task BroadcastClientEventById(int UserID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsUser && item.Key.UserID == UserID.ToString())
                {
                    await SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }


        public async Task BroadcastClientEvent(EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsUser)
                {
                    await SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }


        public async Task BroadcastManagerEventByID(int ManagerID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsManager && item.Key.UserID == ManagerID.ToString())
                {
                    await SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }

        public async Task BroadcastAdminEvent(EventModel data)
        {
            foreach(var item in _UserSocketsPool)
            {
                if(item.Key.IsAdmin)
                {
                    await SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }




        public async Task Handle(WebSocket ws)
        {
            try {
                WebSocketReceiveResult message;
                do
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        message = await ReceiveMessage(ws, memoryStream);
                    }
                } while (message.MessageType != WebSocketMessageType.Close && ws.State == WebSocketState.Open);
                await Close(ws);
            } catch (Exception ex)
            {
                Serilog.Log.Information("Connection with client closed ");
                Serilog.Log.Information(ex.Message);
                Serilog.Log.Information(ex.StackTrace);
                return;
            }
        }


        private async Task<WebSocketReceiveResult> ReceiveMessage(WebSocket ws, Stream memoryStream)
        {
            var readBuffer = new ArraySegment<byte>(new byte[4 * 1024]);
            WebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(readBuffer, CancellationToken.None);
                await memoryStream.WriteAsync(readBuffer.Array, readBuffer.Offset, result.Count, CancellationToken.None);
            } while (!result.EndOfMessage);
            return result;
        }

        public async Task SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }


        public async Task Close(WebSocket ws)
        {
           await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None); 
        }
    }
}
