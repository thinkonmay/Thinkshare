using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Host.Models;
using System.Diagnostics;

namespace Host.Slave
{
    public class DeviceOpen : SlaveState
    {
        public override void SessionInitialize(Slave slave, Session session)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.SESSION_INITIALIZE;
            message.Data = JsonConvert.SerializeObject(message);

            slave.Hub.SendMessage(message);

            SlaveState _state = new OnSession();
            slave.State = _state;
        }

        public override void SessionTerminate(Slave slave)
        {
            Debug.Write("not in session");
        }

        public override void RemoteControlDisconnect(Slave slave)
        {
            Debug.Write("not in session");
        }

        public override void RemoteControlReconnect(Slave slave)
        {
            Debug.Write("not in session");
        }

        public override void SendCommand(Slave Slave, string command)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.COMMAND_LINE_FOWARD;

            Command forward_command = new Command();
            message.Data = JsonConvert.SerializeObject(forward_command);

            Slave.Hub.SendMessage(message);
        }

        public override void UpdateState(Slave slave, DeviceInformation information, DeviceState state)
        {
            slave.Device.Information = information;
            slave.Device.State = state;
        }
    }
}
