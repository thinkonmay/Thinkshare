using SharedHost.Models.Shell;
using Newtonsoft.Json;
using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using SharedHost.Models.Shell;
using SlaveManager.Interfaces;
using System;
using System.Threading.Tasks;

namespace SlaveManager.SlaveDevices.SlaveStates
{
    public class DeviceOpen : ISlaveState
    {
        public async Task SessionInitialize(ISlaveDevice slave, SlaveSession session)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.SESSION_INITIALIZE;
            message.Data = JsonConvert.SerializeObject(session);

            await slave.SendMessage(message);

            var _state = new OnSession();
            slave.ChangeState(_state);
            return;
        }

        public async Task SessionTerminate(ISlaveDevice slave)
        {
            return;
        }

        public async Task RemoteControlDisconnect(ISlaveDevice slave)
        {
            return;
        }

        public async Task RemoteControlReconnect(ISlaveDevice slave)
        {
            return;
        }

        public async Task InitializeShellSession(ISlaveDevice slave, ShellScript script)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.NEW_SHELL_SESSION;
            message.Data = JsonConvert.SerializeObject(script);
            await slave.SendMessage(message);
            return;
        }

        public async Task RejectSlave(ISlaveDevice slave)
        {
            Message msg = new Message();
            msg.From = Module.HOST_MODULE;
            msg.To = Module.AGENT_MODULE;
            msg.Opcode = Opcode.REJECT_SLAVE;
            msg.Data = " ";

            await slave.SendMessage(msg);
            ISlaveState _state = new DeviceDisconnected();
            slave.ChangeState(_state);
        }

        public string GetSlaveState()
        {
            return SlaveServiceState.Open;
        }

        public async Task OnSessionCoreExit(ISlaveDevice slave, int SlaveID)
        {
            return;
        }

    }
}
