using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedHost.Models;
using SharedHost.Interfaces;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace SharedHost.Slave
{
    /// <summary>
    /// 
    /// </summary>
    public class SlaveDevice
    {
        public SlaveState State { get; set; }
        public WebSocket ws { get; set; }

        public SlaveSession session;
        public SlaveDevice(SlaveState _state, WebSocket _ws)
        {
            State = _state;
            ws = _ws;
        }


        public void ChangeState(SlaveState newstate)
        {
            State = newstate;
        }


        private async Task<Message> ReceiveMessage()
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


        public async Task Handle()
        {
            while (ws.State == WebSocketState.Open)
            {
                var message = await ReceiveMessage();
                if (message != null)
                {
                    if (message.To != Module.HOST_MODULE)
                    {
                        if (message.From == Module.AGENT_MODULE)
                        {
                            switch (message.Opcode)
                            {
                                case Opcode.COMMAND_LINE_FOWARD:
                                    {
                                        Command command = JsonConvert.DeserializeObject<Command>(message.Data);
                                        /*send command to admin here*/
                                        break;
                                    }

                            }
                        }
                        else if (message.From == Module.CORE_MODULE)
                        {
                            switch (message.Opcode)
                            {
                                case Opcode.ERROR_REPORT:
                                    break;
                                case Opcode.EXIT_CODE_REPORT:
                                    break;
                            }
                        }
                    }
                }
            }
            var disconnected = new DeviceDisconnected();
            State = disconnected;
        }



        public async Task SendMessage(Message message)
        {

            string msg_string = JsonConvert.SerializeObject(message);

            var bytes = Encoding.UTF8.GetBytes(msg_string);
            var buffer = new ArraySegment<byte>(bytes);
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

        }


        /*state dependent method*/
        public Tuple<bool, string> SessionInitialize(SlaveSession session)
        {
            return  State.SessionInitialize(this, session);
        }

        public Tuple<bool, string> SessionTerminate()
        {
            return State.SessionTerminate(this);
        }

        public Tuple<bool, string> RemoteControlDisconnect()
        {
            return State.RemoteControlDisconnect(this);
        }

        public Tuple<bool, string> RemoteControlReconnect()
        {
            return State.RemoteControlReconnect(this);
        }

        public Tuple<bool, string> SendCommand(int order, string command)
        {
            return State.SendCommand(this, order, command);
        }

        public Tuple<bool, string> RejectSlave()
        {
            return State.RejectSlave(this);
        }
    }
}
