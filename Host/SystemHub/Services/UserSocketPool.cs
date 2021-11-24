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

namespace SystemHub.Services
{
    public class UserSocketPool : IUserSocketPool
    {
        private readonly ConcurrentDictionary<AuthenticationResponse, WebSocket> _UserSocketsPool;        
        
        public UserSocketPool(SystemConfig config)
        {
            _UserSocketsPool =    new ConcurrentDictionary<AuthenticationResponse, WebSocket>();

            Task.Run(() => ConnectionHeartBeat());
            Task.Run(() => ConnectionStateCheck());
        }



        public async Task ConnectionHeartBeat()
        {
            try
            {                
                foreach (var socket in _UserSocketsPool)
                {
                    if(socket.Value.State == WebSocketState.Open)
                    {
                        SendMessage(socket.Value,"ping");
                    }
                }
                Thread.Sleep(30*1000);
            }catch
            {
                await ConnectionHeartBeat();
            }
        }

        public async Task ConnectionStateCheck()
        {
            try
            {                
                foreach (var socket in _UserSocketsPool)
                {
                    if(socket.Value.State == WebSocketState.Closed) 
                    {
                        _UserSocketsPool.TryRemove(socket);
                    }
                }
                Thread.Sleep(100);
            }catch
            {
                await ConnectionStateCheck();
            }
        }


        public void AddtoPool(AuthenticationResponse resp,WebSocket session)
        {
            _UserSocketsPool.TryAdd(resp,session);
        }




        public void BroadcastClientEventById(int UserID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsUser && item.Key.UserID == UserID.ToString())
                {
                    SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }


        public void BroadcastClientEvent(EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsUser)
                {
                    SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }


        public void BroadcastManagerEventByID(int ManagerID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsManager && item.Key.UserID == ManagerID.ToString())
                {
                    SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }

        public void BroadcastAdminEvent(EventModel data)
        {
            foreach(var item in _UserSocketsPool)
            {
                if(item.Key.IsAdmin)
                {
                    SendMessage(item.Value, JsonConvert.SerializeObject(data));
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
                        if (message.Count > 0)
                        {
                            var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                        }
                    }
                } while (message.MessageType != WebSocketMessageType.Close && ws.State == WebSocketState.Open);
            } catch 
            {
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

        public void SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
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
