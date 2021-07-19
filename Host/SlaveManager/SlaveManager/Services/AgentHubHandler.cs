using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using SlaveManager.Interfaces;
using SlaveManager.Slave;
using SharedHost.Models;

namespace SlaveManager.Services
{
    public class AgentHubHandler : IAgentHubHandler
    {
        public ConcurrentDictionary<int, SlaveDevice> SlaveList = new ConcurrentDictionary<int, SlaveDevice>();

        public async Task Handle(WebSocket ws)
        {
            while (ws.State == WebSocketState.Open)
            {
                var message = await ReceiveMessage(ws);
                if (message != null)
                {
                    switch (message.Opcode)
                    {
                        case Opcode.REGISTER_SLAVE:
                            var _slave = _handleSlaveRegistration(ws, JsonConvert.DeserializeObject<Device>(message.Data))
                                .Result;
                            if (_slave != null)
                            {
                                Message accept = new Message();
                                accept.From = Module.AGENT_MODULE;
                                accept.To = Module.AGENT_MODULE;
                                accept.Opcode = Opcode.SLAVE_ACCEPTED;

                                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(accept));
                                var buffer = new ArraySegment<byte>(bytes);

                                await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            else
                            {
                                Message deny = new Message();
                                deny.From = Module.AGENT_MODULE;
                                deny.To = Module.AGENT_MODULE;
                                deny.Opcode = Opcode.DENY_SLAVE;

                                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(deny));
                                var buffer = new ArraySegment<byte>(bytes);

                                await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            break;
                    }
                }
            }
        }

        public async Task<Message> ReceiveMessage(WebSocket ws)
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

        public async Task SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }


        private Task<SlaveDevice> _handleSlaveRegistration(WebSocket ws, Device device)
        {
            return Task.Run(() =>
            {
                SlaveDevice slave;
                if(SlaveList.TryGetValue(device.Information.ID, out slave))
                {

                    SlaveState _open = new DeviceOpen();
                    slave.ws = ws;
                    slave.State = _open;
                    /*invoke admin for acception*/

                    Task.Run(() => slave.Handle());
                    return slave;
                }
                else /*invoke admin for acception*/
                {
                    return null;
                }
            });
        }
    }
}
