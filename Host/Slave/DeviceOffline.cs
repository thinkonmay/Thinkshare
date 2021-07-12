using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Host.Models;

namespace Host.Slave
{
    public class DeviceOffline : SlaveState
    { 
        public override void SessionInitialize(Slave slave, Session session)
        {
            Debug.Write("Device is offline");
        }

        public override void SessionTerminate(Slave slave)
        {
            Debug.Write("Device is offline");
        }

        public override void RemoteControlDisconnect(Slave slave)
        {
            Debug.Write("Device is offline");
        }

        public override void RemoteControlReconnect(Slave slave)
        {
            Debug.Write("Device is offline");
        }

        public override void SendCommand(Slave slave, string command)
        {
            Debug.Write("Device is offline");
        }

        public override void UpdateState(Slave slave, DeviceInformation information, DeviceState state)
        {
            Debug.Write("Device is offline");
        }
    }
}
