using SharedHost.Models.Command;
using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Interfaces;
using System.Net.WebSockets;
using System.Threading.Tasks;
using SlaveManager.SlaveDevices.SlaveStates;
using System.IO;
using System.Text;
using System.Threading;
using System;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using SharedHost.Models.Error;
using SlaveManager.Services;
using SharedHost;

namespace SlaveManager.SlaveDevices
{
    public class SlaveDevice : ISlaveDevice
    {
        public ISlaveState State { get; set; }
        public WebSocket ws { get; set; }
        public ConductorSocket _conductor { get; set; }
        public SlaveDevice(SystemConfig config)
        {
            _conductor = new ConductorSocket(config);
            ws = null;
            State = new DeviceDisconnected();
        }




        public async Task KeepReceiving(int SlaveID)
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
                                await OnHostMessage(messageForm);
                            }
                        }
                    }
                } while (ws.State == WebSocketState.Open);
            } catch (Exception)
            { }
            await _conductor.ReportSlaveDisconnected(SlaveID);
            State = new DeviceDisconnected();
        }


        private async Task OnHostMessage(MessageWithID messageForm)
        { 
            try
            {
                if (messageForm.From == (int)Module.AGENT_MODULE)
                {
                    switch (messageForm.Opcode)
                    {
                        case (int)Opcode.ERROR_REPORT:
                        {
                            var error = JsonConvert.DeserializeObject<ReportedError>(messageForm.Data);
                            error.SlaveID = messageForm.SlaveID;
                            error.Module = (int)Module.AGENT_MODULE;
                            System.Console.WriteLine(JsonConvert.SerializeObject(error));
                            break;
                        }
                        case (int)Opcode.SESSION_CORE_EXIT:
                        {
                            await State.OnSessionCoreExit(this, messageForm.SlaveID);
                            break;
                        }
                        case (int)Opcode.END_SHELL_SESSION:
                        {
                            var output = JsonConvert.DeserializeObject<ShellOutput>(messageForm.Data);
                            output.SlaveID = messageForm.SlaveID;
                            await EndShellSession(output);
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
                            var errabs = JsonConvert.DeserializeObject<ReportedError>(messageForm.Data);
                            errabs.SlaveID = messageForm.SlaveID;
                            errabs.Module = (int)Module.CORE_MODULE;
                            System.Console.WriteLine(JsonConvert.SerializeObject(errabs));
                            break;
                        }
                    }
                }
            }catch(Exception)
            { }
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










        public async Task OnRemoteControlDisconnected(int SlaveID)
        {
            State = new OnSessionOffRemote();
            await _conductor.ReportRemoteControlDisconnected(SlaveID);
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

        public async Task OnSessionCoreExit(int SlaveID)
        {
            await State.OnSessionCoreExit(this, SlaveID);
        }











        public async Task InitializeShellSession(ShellScript script)
        {
            await State.InitializeShellSession(this, script);
        }

        public async Task EndShellSession(ShellOutput output)
        {
            await _conductor.LogShellOutput(output);
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
