using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using SharedHost.Models;

namespace SlaveManager.SlaveStates
{
    public class DeviceOpen : SlaveState
    {
        public override Tuple<bool, string> SessionInitialize(SlaveDevice slave, SlaveSession session)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.SESSION_INITIALIZE;
            message.Data = JsonConvert.SerializeObject(session);

            _ = Task.Run(() => slave.SendMessage(message));

            SlaveState _state = new OnSession();
            slave.State = _state;

            return new Tuple<bool, string>(true, "session initialized");
        }

        public override Tuple<bool, string> SessionTerminate(SlaveDevice slave)
        {
            return new Tuple<bool, string>(false, "not in session");
        }

        public override Tuple<bool, string> RemoteControlDisconnect(SlaveDevice slave)
        {
            return new Tuple<bool, string>(false, "not in sessiony");
        }

        public override Tuple<bool, string> RemoteControlReconnect(SlaveDevice slave)
        {
            return new Tuple<bool, string>(false, "not in session");
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

            _ = Task.Run(() =>slave.SendMessage(msg));
            return new Tuple<bool, string>(true, "rejected slave");
        }
    }
}
