using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SharedHost.Models;

<<<<<<< Updated upstream:Host/SlaveManager/SlaveManager/Slave/DeviceDisconnected.cs
namespace SharedHost.Slave
=======
namespace SlaveManager.SlaveStates
>>>>>>> Stashed changes:Host/SlaveManager/SlaveManager/SlaveStates/DeviceDisconnected.cs
{
    public class DeviceDisconnected : SlaveState
    {
        public override Tuple<bool, string> SessionInitialize(SlaveDevice slave, SlaveSession session)
        {
            return new Tuple<bool, string>(false,"device disconnected");
        }

        public override Tuple<bool, string> SessionTerminate(SlaveDevice slave)
        {
            return new Tuple<bool, string>(false, "device disconnected");
        }

        public override Tuple<bool, string> RemoteControlDisconnect(SlaveDevice slave)
        {
            return new Tuple<bool, string>(false, "device disconnected");
        }

        public override Tuple<bool, string> RemoteControlReconnect(SlaveDevice slave)
        {
            return new Tuple<bool, string>(false, "device disconnected");
        }

        public override Tuple<bool, string> SendCommand(SlaveDevice slave,int order, string command)
        {
            return new Tuple<bool, string>(false, "device disconnected");
        }


        public override Tuple<bool, string> RejectSlave(SlaveDevice slave)
        {
            return new Tuple<bool, string>(true, "device disconnected");
        }
    }
}
