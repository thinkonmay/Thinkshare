using Newtonsoft.Json;
using SharedHost.Models.Shell;
using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using WorkerManager.Interfaces;
using System;
using System.Threading.Tasks;

namespace WorkerManager.SlaveDevices.SlaveStates
{
    public class OnSession : ISlaveState
    {
        public async Task SessionInitialize(SlaveDevice slave, SlaveSession session)
        {
            Console.WriteLine("Already In Session");
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

            var _state = new DeviceOpen();
            slave.ChangeState(_state);


            return;
        }

        public async Task RemoteControlDisconnect(SlaveDevice slave)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.DISCONNECT_REMOTE_CONTROL;
            message.Data = " ";

            slave.ChangeState(new OnSessionOffRemote());
            await slave.SendMessage(message);


            return;
        }

        public async Task RemoteControlReconnect(SlaveDevice slave)
        {
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
            msg.Data = " ";

            await slave.SendMessage(msg);
            ISlaveState _state = new DeviceDisconnected();
            slave.ChangeState(_state);

            return;
        }

        public string GetSlaveState()
        {
            return SlaveServiceState.OnSession;
        }

        public async Task OnSessionCoreExit(SlaveDevice slave, int SlaveID)
        {
            await slave.OnRemoteControlDisconnected(SlaveID);
        }
    }
}
