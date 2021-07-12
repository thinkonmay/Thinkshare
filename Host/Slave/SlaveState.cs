using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Host.Interfaces;
using Host.Models;

namespace Host.Slave
{
    public abstract class SlaveState
    {
        public abstract void SessionInitialize(Slave slave, Session session);

        public abstract void SessionTerminate(Slave slave);

        public abstract void RemoteControlDisconnect(Slave slave);

        public abstract void RemoteControlReconnect(Slave slave);

        public abstract void SendCommand(Slave slave, string command);

        public abstract void UpdateState(Slave slave, DeviceInformation information, DeviceState state);
    }
}
