using Newtonsoft.Json;
using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using SlaveManager.Interfaces;
using System;
using System.Threading.Tasks;

namespace SlaveManager.SlaveDevices.SlaveStates
{
    public class OnSessionOffRemote : ISlaveState
    {
        public async Task SessionInitialize(ISlaveDevice slave, SlaveSession session)
        {
            return;
        }

        public async Task SessionTerminate(ISlaveDevice slave)
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

        public async Task RemoteControlDisconnect(ISlaveDevice slave)
        {
            return;
        }

        public async Task RemoteControlReconnect(ISlaveDevice slave)
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



        public async Task InitializeShellSession(ISlaveDevice slave, int order)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.NEW_SHELL_SESSION;

            ForwardScript forward_script = new ForwardScript();
            forward_script.ProcessID = order;
            forward_script.Script = " ";

            message.Data = JsonConvert.SerializeObject(forward_script);
            await slave.SendMessage(message);
            return;
        }

        public async Task RejectSlave(ISlaveDevice slave)
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

        public async Task OnSessionCoreExit(ISlaveDevice slave, int SlaveID)
        {
            return;
        }
    }
}
