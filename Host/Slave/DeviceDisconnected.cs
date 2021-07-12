using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Host.Models;

namespace Host.Slave
{
    public class DeviceDisconnected : SlaveState
    {
        public override void SessionInitialize(Slave slave, Session session)
        {
            Debug.Write("device disconnected");
        }

        public override void SessionTerminate(Slave slave)
        {
            Debug.Write("device disconnected");
        }

        public override void RemoteControlDisconnect(Slave slave)
        {
            Debug.Write("device disconnected");
        }

        public override void RemoteControlReconnect(Slave slave)
        {
            Debug.Write("device disconnected");
        }

        public override void SendCommand(Slave slave, string command)
        {
            Debug.Write("device disconnected");
        }

        public override void UpdateState(Slave slave, DeviceInformation information, DeviceState state)
        {
            slave.Device.Information = information;
            slave.Device.State = state;

            SlaveState _state = new DeviceOpen();
            slave.State = _state;
        }
    }
}
