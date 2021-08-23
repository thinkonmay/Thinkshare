using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Interfaces;
using System;
using System.Threading.Tasks;

namespace SlaveManager.SlaveDevices.SlaveStates
{
    public class OnSession : ISlaveState
    {
        public async Task SessionInitialize(ISlaveDevice slave, SlaveSession session)
        {
            Console.WriteLine("Already In Session");
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

            var _state = new DeviceOpen();
            slave.ChangeState(_state);


            return;
        }

        public async Task RemoteControlDisconnect(ISlaveDevice slave)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.DISCONNECT_REMOTE_CONTROL;
            message.Data = " ";

            await slave.SendMessage(message);


            return;
        }

        public async Task RemoteControlReconnect(ISlaveDevice slave)
        {
            return;
        }

        public async Task SendCommand(ISlaveDevice slave, int order, string command)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.COMMAND_LINE_FORWARD;

            ForwardCommand forward_command = new ForwardCommand();
            forward_command.ProcessID = order;
            forward_command.CommandLine = command;

            message.Data = JsonConvert.SerializeObject(forward_command);

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

            return;
        }

        public string GetSlaveState()
        {
            return SlaveServiceState.OnSession;
        }
    }
}
