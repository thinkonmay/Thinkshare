using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Host.Models;
using Newtonsoft.Json;

namespace Host.Slave
{
    public class OnSessionOffRemote : SlaveState
    {
        public override void SessionInitialize(Slave slave, Session session)
        {
            Debug.Write("not in open state");
        }

        public override void SessionTerminate(Slave slave)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.SESSION_TERMINATION;
            message.Data = null;

            slave.Hub.SendMessage(message);

            SlaveState State = new DeviceOpen();
            slave.State = State;
        }

        public override void RemoteControlDisconnect(Slave slave)
        {
            Debug.Write("already in off remote state");
        }

        public override void RemoteControlReconnect(Slave slave)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.RECONNECT_REMOTE_CONTROL;
            message.Data = null;

            slave.Hub.SendMessage(message);

            SlaveState State = new OnSession();
            slave.State = State; 

        }

        public override void SendCommand(Slave slave, string command)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.COMMAND_LINE_FOWARD;

            Command forward_command = new Command();
            message.Data = JsonConvert.SerializeObject(forward_command);

            slave.Hub.SendMessage(message);
        }

        public override void UpdateState(Slave slave, DeviceInformation information, DeviceState state)
        {
            slave.Device.Information = information;
            slave.Device.State = state;
        }
    }
}
