using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SharedHost.Models;
using Newtonsoft.Json;

namespace SharedHost.Slave
{
    public class OnSession : SlaveState
    {
        public override Tuple<bool, string> SessionInitialize(SlaveDevice slave, SlaveSession session)
        {
            return new Tuple<bool, string>(false, "already on session");
        }

        public override Tuple<bool, string> SessionTerminate(SlaveDevice slave)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.SESSION_TERMINATION;
            message.Data = null;

            _ = Task.Run(()=> slave.SendMessage(message));

            SlaveState State = new DeviceOpen();
            slave.State = State;


            return new Tuple<bool, string>(true, "session terminated sucessfully");
        }

        public override Tuple<bool, string> RemoteControlDisconnect(SlaveDevice slave)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.DISCONNECT_REMTOE_CONTROL;
            message.Data = null;

            _ = Task.Run(() => slave.SendMessage(message));


            return new Tuple<bool, string>(true, "reconnected remote control");
        }

        public override Tuple<bool, string> RemoteControlReconnect(SlaveDevice slave)
        {
            return new Tuple<bool, string>(false, "not in off remote state");
        }

        public override Tuple<bool, string> SendCommand(SlaveDevice slave, int order, string command)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.COMMAND_LINE_FOWARD;

            Command forward_command = new Command();
            forward_command.Order = order;
            forward_command.Commnad = command;

            message.Data = JsonConvert.SerializeObject(forward_command);

            _ = Task.Run(() => slave.SendMessage(message));


            return new Tuple<bool, string>(true, "command send");
        }

        public override Tuple<bool, string> RejectSlave(SlaveDevice slave)
        {
            Message msg = new Message();
            msg.From = Module.HOST_MODULE;
            msg.To = Module.AGENT_MODULE;
            msg.Opcode = Opcode.REJECT_SLAVE;

            _ = Task.Run(() => slave.SendMessage(msg));


            return new Tuple<bool, string>(true, "slave rejected");
        }
    }
}
