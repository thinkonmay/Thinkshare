using SharedHost.Models.Shell;
using Newtonsoft.Json;
using SharedHost.Models;
using WorkerManager.Interfaces;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WorkerManager.SlaveDevices.SlaveStates;
using System.IO;
using System.Text;
using System.Threading;
using System;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using SharedHost.Models.Error;
using WorkerManager.Services;
using SharedHost;
using RestSharp;

namespace WorkerManager.SlaveDevices
{
    public class SlaveDevice 
    {
        public ISlaveState State { get; set; }
        public RestClient ws { get; set; }
        public ConductorSocket _conductor { get; set; }
        public SlaveDevice(SystemConfig config)
        {
            _conductor = new ConductorSocket(config);
            State = new DeviceDisconnected();
        }




        public async Task KeepReceiving(int SlaveID)
        {

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
                            break;
                        }
                        case (int)Opcode.SESSION_CORE_EXIT:
                        {

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
                            break;
                        }
                    }
                }
            }catch(Exception ex)
            {
                Serilog.Log.Information("Failed to execute message: {reason}.",ex.Message);
            }
        }


        public async Task Send(string message)
        {

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
