using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Interfaces;
using System.Net.WebSockets;
using SlaveManager.Services;
using System.Threading.Tasks;
using SlaveManager.SlaveDevices.SlaveStates;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public IConnection connection { get; set; }
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
                            var json_message = JsonConvert.DeserializeObject<MessageWithID>(receivedMessage);

                            if (json_message != null)
                            {
                                if (json_message.To != Module.HOST_MODULE)
                                {
                                    if (json_message.From == Module.AGENT_MODULE)
                                    {
                                        switch (json_message.Opcode)
                                        {
                                            case Opcode.COMMAND_LINE_FORWARD:
                                                {
                                                    var cmd = JsonConvert.DeserializeObject<ReceiveCommand>(json_message.Data);
                                                    await _admin.LogSlaveCommandLine(json_message.SlaveID, cmd);    
                                                    /*send command to admin here*/
                                                    break;
                                                }
                                            case Opcode.ERROR_REPORT:
                                                {
                                                    GeneralError err = new GeneralError(JsonConvert.DeserializeObject<GeneralErrorAbsTime>(json_message.Data));
                                                    await _admin.ReportAgentError(err);
                                                    break;
                                                }

                                        }
                                    }
                                    else if (json_message.From == Module.CORE_MODULE)
                                    {
                                        switch (json_message.Opcode)
                                        {
                                            case Opcode.ERROR_REPORT:
                                                {
                                                    var errabs = JsonConvert.DeserializeObject<GeneralErrorAbsTime>(json_message.Data);
                                                    var err = new GeneralError(errabs);
                                                    await _admin.ReportSessionCoreError(err);
                                                    break;
                                                }
                                            case Opcode.EXIT_CODE_REPORT:
                                                {
                                                    var abs = JsonConvert.DeserializeObject<SessionCoreExitAbsTime> ( json_message.Data );
                                                    var err = new SessionCoreExit(abs);

                                                    await _admin.ReportSessionCoreExit(json_message.SlaveID, err);

                                                    //TODO: add to db
                                                    break;
                                                }
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
