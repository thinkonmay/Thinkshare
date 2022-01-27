using SharedHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using SystemHub.Interfaces;
using System.Threading.Tasks;
using SharedHost.Auth;
using SharedHost.Models.Message;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Options;
using SystemHub.Models;
using System.Collections.Concurrent;
using SharedHost.Logging;

namespace SystemHub.Services
{
    public class UserSocketPool : IUserSocketPool
    {
        private readonly ConcurrentDictionary<UserHubCredential,WebSocket> _UserSocketsPool;        

        private readonly ILog _log;
        
        public UserSocketPool(IOptions<SystemConfig> config,
                              ILog log)
        {
            _UserSocketsPool = new ConcurrentDictionary<UserHubCredential, WebSocket>();
            _log = log;
            Task.Run(() => ConnectionHeartBeat());
            Task.Run(() => ConnectionStateCheck());
        }



        public async Task ConnectionHeartBeat()
        {
            while (true)
            {
                foreach (var socket in _UserSocketsPool)
                {
                    if(socket.Value.State == WebSocketState.Open)
                    {
                        try {await SendMessage(socket.Value,"ping"); }
                        catch {  }
                    }
                }
                Thread.Sleep(30*1000);
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
                            _UserSocketsPool.TryRemove(socket.Key, out var currupted);
                        }
                    }
                    Thread.Sleep(100);
                }
            }catch (Exception ex)
            {
                Thread.Sleep(100);
                await ConnectionStateCheck();
            }
        }


        public async Task AddtoPool(AuthenticationResponse resp,WebSocket session)
        {
            var credential = new UserHubCredential(resp,session);
            _UserSocketsPool.AddOrUpdate(credential,session,(x,y) => session);
            await Handle(session);
            _UserSocketsPool.TryRemove(credential, out var removedSession);
        }




        public async Task BroadcastClientEventById(int UserID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsUser && item.Key.UserID == UserID.ToString())
                {
                    await SendMessage(item.Value , JsonConvert.SerializeObject(data));
                }
            }
        }


        public async Task BroadcastClientEvent(EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsUser)
                {
                    await SendMessage(item.Value , JsonConvert.SerializeObject(data));
                }
            }
        }


        public async Task BroadcastManagerEventByID(int ManagerID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsManager && item.Key.UserID == ManagerID.ToString())
                {
                    await SendMessage(item.Value , JsonConvert.SerializeObject(data));
                }
            }
        }

        public async Task BroadcastAdminEvent(EventModel data)
        {
            foreach(var item in _UserSocketsPool)
            {
                if(item.Key.IsAdmin)
                {
                    await SendMessage(item.Value , JsonConvert.SerializeObject(data));
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
                _log.Error("Connection with client closed",ex);
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
            try
            {
                await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            } catch { _log.Information("Fail to send to user"); }
        }


        public async Task Close(WebSocket ws)
        {
           await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None); 
        }
    }
}
