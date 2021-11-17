using Newtonsoft.Json;
using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using WorkerManager.Interfaces;
using System;
using System.Threading.Tasks;
using SharedHost.Models.Shell;

namespace WorkerManager.SlaveDevices.SlaveStates
{
    public class OnSessionOffRemote : ISlaveState
    {
        public async Task SessionInitialize(SlaveDevice slave, SlaveSession session)
        {
            return;
        }

        public async Task SessionTerminate(SlaveDevice slave)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.SESSION_TERMINATE;
            message.Data = " ";

            await slave.SendMessage(message);

            var State = new DeviceOpen();
            slave.ChangeState(State);

            return;
        }

        public async Task RemoteControlDisconnect(SlaveDevice slave)
        {
            return;
        }

        public async Task RemoteControlReconnect(SlaveDevice slave)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.RECONNECT_REMOTE_CONTROL;
            message.Data = " ";


            slave.ChangeState(new OnSession());
            await slave.SendMessage(message);


            return;
        }



        public async Task InitializeShellSession(SlaveDevice slave, ShellScript script)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.NEW_SHELL_SESSION;
            message.Data = JsonConvert.SerializeObject(script);
            await slave.SendMessage(message);
            return;
        }

        public async Task RejectSlave(SlaveDevice slave)
        {
            Message msg = new Message();
            msg.From = Module.HOST_MODULE;
            msg.To = Module.AGENT_MODULE;
            msg.Opcode = Opcode.REJECT_SLAVE;

            await slave.SendMessage(msg);
            ISlaveState _state = new DeviceDisconnected();
            slave.ChangeState(_state);
            return;
        }

        public string GetSlaveState()
        {
            return SlaveServiceState.OffRemote;
        }

        public async Task OnSessionCoreExit(SlaveDevice slave, int SlaveID)
        {
            return;
        }
    }
}
