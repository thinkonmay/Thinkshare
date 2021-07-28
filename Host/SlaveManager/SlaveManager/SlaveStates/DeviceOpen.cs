using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using SharedHost.Models;
using SlaveManager.Interfaces;
using SlaveManager.Models;

namespace SlaveManager.SlaveStates
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

            ISlaveState _state = new OnSession();
            slave.ChangeState( _state);
            return;
        }

        public async Task SessionTerminate(ISlaveDevice slave)
        {
            Console.WriteLine("Not In Session");
            return;
        }

        public async Task RemoteControlDisconnect(ISlaveDevice slave)
        {
            Console.WriteLine("Not In Session");
            return;
        }

        public async Task RemoteControlReconnect(ISlaveDevice slave)
        {
            Console.WriteLine("Not In Session");
            return;
        }

        public async Task SendCommand(ISlaveDevice slave, int order, string command)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.COMMAND_LINE_FOWARD;

            Command forward_command = new Command();
            forward_command.Order = order;
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

            ISlaveState _state = new DeviceDisconnected();
            slave.ChangeState(_state);

            await slave.SendMessage(msg);
            return;
        }

        public string GetSlaveState()
        {
            return "Device Open";
        }
    }
}
