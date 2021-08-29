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

            await slave.SendMessage(message);


            return;
        }



        public async Task InitializeCommandlineSession(ISlaveDevice slave, int order)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.NEW_COMMAND_LINE_SESSION;

            ForwardCommand forward_command = new ForwardCommand();
            forward_command.ProcessID = order;
            forward_command.CommandLine = " ";

            message.Data = JsonConvert.SerializeObject(forward_command);
            await slave.SendMessage(message);
            return;
        }

        public async Task TerminateCommandlineSession(ISlaveDevice slave, int order)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.END_COMMAND_LINE_SESSION;

            ForwardCommand forward_command = new ForwardCommand();
            forward_command.ProcessID = order;
            forward_command.CommandLine = " ";

            message.Data = JsonConvert.SerializeObject(forward_command);
            await slave.SendMessage(message);
            return;
        }


        public async Task SendCommand(ISlaveDevice slave, ForwardCommand command)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.COMMAND_LINE_FORWARD;
            message.Data = JsonConvert.SerializeObject(command);

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
