using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Host.Interfaces;
using Host.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Host.Data;

namespace Host.Services
{
    public class AgentHub : IAgentHub
    {
        private readonly ApplicationDbContext _db;

        public AgentHub(ApplicationDbContext db)
        {
            _db = db;
        }

        private async Task<Message> ReceiveMessage(WebSocket ws)
        {
            var buffer = new Memory<byte>();
            var request = await ws.ReceiveAsync(buffer, CancellationToken.None);

            if (request.MessageType == WebSocketMessageType.Text)
            {
                var msg = Encoding.UTF8.GetString(buffer.ToArray());
                return JsonConvert.DeserializeObject<Message>(msg);
            }

            return null;
        }


        public async Task Handle(WebSocket ws)
        {
            while (ws.State == WebSocketState.Open)
            {
                var message = await ReceiveMessage(ws);
                if (message != null)
                {
                    if (message.To != Module.HOST_MODULE)
                    {
                        continue;
                    }

                    switch (message.Opcode)
                    {
                        case Opcode.REGISTER_SLAVE:
                            var deviceInfo = JsonConvert.DeserializeObject<DeviceInformation>(message.Data);

                            break;
                        case Opcode.UPDATE_SLAVE_STATE:
                            break;
                        case Opcode.SESSION_INITIALIZE_CONFIRM:
                            break;
                        case Opcode.SESSION_TERMINATE_CONFIRM:
                            break;
                        case Opcode.DISCONNECT_REMOTE_CONTROL_CONFIRM:
                            break;
                        case Opcode.RECONNECT_REMOTE_CONTROL_CONFIRM:
                            break;
                    }  
                }
            }
        }



        public async Task SendMessage(WebSocket ws, Message message)
        {
            string msg_string = JsonConvert.SerializeObject(message);

            var bytes = Encoding.UTF8.GetBytes(msg_string);
            var buffer = new ArraySegment<byte>(bytes);
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

        }
    }
}