using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Interfaces;
using System.Net.WebSockets;
using SlaveManager.Services;
using System.Threading.Tasks;
using SlaveManager.SlaveDevices.SlaveStates;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System;
using SlaveManager.Models;

namespace SlaveManager.SlaveDevices
{
    public interface ISlaveDevice
    {
        void ChangeState(ISlaveState newstate);
        string GetSlaveState();
        Task SendMessage(Message message);
        Task SessionInitialize(SlaveSession session);
        Task RemoteControlDisconnect();
        Task RemoteControlReconnect();
        Task SendCommand(int order, string command);
        Task RejectSlave();
    }
    /// <summary>
    /// 
    /// </summary>
    public class SlaveDevice : ISlaveDevice
    {
        public ISlaveState State { get; set; }
        public WebSocket ws { get; set; }
        private readonly IAdmin _admin;
        public SlaveDevice(IAdmin admin)
        {
            ws = null;
            _admin = admin;
            State = new DeviceDisconnected();
        }




        public async Task<bool> KeepReceiving()
        {
            WebSocketReceiveResult message;
            try
            {
                do
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        message = await ReceiveMessage(memoryStream);
                        if (message.Count > 0)
                        {
                            var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                            var messageForm = JsonConvert.DeserializeObject<MessageWithID>(receivedMessage);

                            if (messageForm.To == (int)Module.HOST_MODULE)
                            {
                                if (messageForm.From == (int)Module.AGENT_MODULE)
                                {
                                    switch (messageForm.Opcode)
                                    {
                                        case (int)Opcode.COMMAND_LINE_FORWARD:
                                        {
                                            var cmd = JsonConvert.DeserializeObject<ReceiveCommand>(messageForm.Data);
                                            await _admin.LogSlaveCommandLine(messageForm.SlaveID, cmd);
                                            break;
                                        }
                                        case (int)Opcode.ERROR_REPORT:
                                        {
                                            var error = JsonConvert.DeserializeObject<GeneralErrorAbsTime>(messageForm.Data);

                                            if(error.ErrorMessage == ErrorMessage.UNKNOWN_SESSION_CORE_EXIT)
                                            {
                                                await _admin.ReportRemoteControlDisconnected(messageForm.SlaveID);
                                                var state = new OnSessionOffRemote();
                                                ChangeState(state);
                                                break;
                                            }
                                            await _admin.ReportAgentError(error, messageForm.SlaveID);
                                            break;
                                        }

                                    }
                                }
                                else if (messageForm.From == (int)Module.CORE_MODULE)
                                {
                                    switch (messageForm.Opcode)
                                    {
                                        case (int)Opcode.ERROR_REPORT:
                                        {
                                            var errabs = JsonConvert.DeserializeObject<GeneralErrorAbsTime>(messageForm.Data);
                                            await _admin.ReportSessionCoreError(errabs, messageForm.SlaveID);
                                            break;
                                        }
                                        case (int)Opcode.EXIT_CODE_REPORT:
                                        {
                                            var abs = JsonConvert.DeserializeObject<SessionCoreExitAbsTime> ( messageForm.Data );
                                            await _admin.ReportSessionCoreExit(messageForm.SlaveID, abs);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                } while (ws.State == WebSocketState.Open);
            } catch (WebSocketException)
            {
                return true;
            }

            return false;
        }

        public async Task Send(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }

        private async Task<WebSocketReceiveResult> ReceiveMessage(Stream memoryStream)
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


        public void ChangeState(ISlaveState newstate)
        {
            State = newstate;
        }


        /*state dependent method*/
        public async Task SessionInitialize(SlaveSession session)
        {
            await State.SessionInitialize(this, session);
        }

        public async Task SessionTerminate()
        {
            await State.SessionTerminate(this);
        }

        public async Task RemoteControlDisconnect()
        {
            await State.RemoteControlDisconnect(this);
        }

        public async Task RemoteControlReconnect()
        {
            await State.RemoteControlReconnect(this);
        }

        public async Task SendCommand(int order, string command)
        {
            await State.SendCommand(this, order, command);
        }

        public async Task RejectSlave()
        {
            await State.RejectSlave(this);
        }

        public Task SendMessage(Message message)
        {
            return Send(JsonConvert.SerializeObject(message));
        }

        public string GetSlaveState()
        {
            return State.GetSlaveState();
        }
    }
}
