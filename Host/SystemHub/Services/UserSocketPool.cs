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

namespace SystemHub.Services
{
    public class UserSocketPool : IUserSocketPool
    {
        private readonly List<UserHubCredential> _UserSocketsPool;        
        
        public UserSocketPool(IOptions<SystemConfig> config)
        {
            _UserSocketsPool = new List<UserHubCredential>();

            Task.Run(() => ConnectionHeartBeat());
            Task.Run(() => ConnectionStateCheck());
        }



        public async Task ConnectionHeartBeat()
        {
            while (true)
            {
                foreach (var socket in _UserSocketsPool)
                {
                    if(socket.websocket.State == WebSocketState.Open)
                    {
                        try {await SendMessage(socket.websocket,"ping"); }
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
                        if(socket.websocket.State == WebSocketState.Closed) 
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
            var credential = new UserHubCredential(resp,session);
            _UserSocketsPool.Add(credential);
            await Handle(session);
            _UserSocketsPool.RemoveAll(x=>x.rand == credential.rand);
        }




        public async Task BroadcastClientEventById(int UserID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.IsUser && item.UserID == UserID.ToString())
                {
                    await SendMessage(item.websocket , JsonConvert.SerializeObject(data));
                }
            }
        }


        public async Task BroadcastClientEvent(EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.IsUser)
                {
                    await SendMessage(item.websocket , JsonConvert.SerializeObject(data));
                }
            }
        }


        public async Task BroadcastManagerEventByID(int ManagerID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.IsManager && item.UserID == ManagerID.ToString())
                {
                    await SendMessage(item.websocket , JsonConvert.SerializeObject(data));
                }
            }
        }

        public async Task BroadcastAdminEvent(EventModel data)
        {
            foreach(var item in _UserSocketsPool)
            {
                if(item.IsAdmin)
                {
                    await SendMessage(item.websocket , JsonConvert.SerializeObject(data));
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
            try
            {
                await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            } catch { Serilog.Log.Information("Fail to send to user"); }
        }


        public async Task Close(WebSocket ws)
        {
           await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None); 
        }
    }
}
