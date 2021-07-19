using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedHost.Models;

namespace SlaveManager.Slave
{
    public class OnSessionOffRemote : SlaveState
    {
        public OnSessionOffRemote()
        {
            this.state = "On Idle Session";
        }
        public override Tuple<bool, string> SessionInitialize(SlaveDevice slave, SlaveSession session)
        {
            return new Tuple<bool, string>(false, "already in session");
        }

        public override Tuple<bool, string> SessionTerminate(SlaveDevice slave)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.SESSION_TERMINATION;
            message.Data = null;

            _ = Task.Run(() => slave.SendMessage(message));

            SlaveState State = new DeviceOpen();
            slave.State = State;

            return new Tuple<bool, string>(true, "session terminated sucessfully");
        }

        public override Tuple<bool, string> RemoteControlDisconnect(SlaveDevice slave)
        {
            return new Tuple<bool, string>(false, "remote control already disconnected");
        }

        public override Tuple<bool, string> RemoteControlReconnect(SlaveDevice slave)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.RECONNECT_REMOTE_CONTROL;
            message.Data = null;

            _ = Task.Run(() => slave.SendMessage(message));

            SlaveState State = new OnSession();
            slave.State = State;


            return new Tuple<bool, string>(true, "reconnected remotecontrol");
        }

        public override Tuple<bool, string> SendCommand(SlaveDevice slave,int order, string command)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.COMMAND_LINE_FOWARD;

            Command forward_command = new Command();
            forward_command.Order = order;
            forward_command.CommandLine = command;

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
