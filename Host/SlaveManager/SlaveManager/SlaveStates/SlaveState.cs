using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models;
using SharedHost.Services;

namespace SharedHost.Slave
{
    public abstract class SlaveState
    {
        public AgentHubHandler Hub;

        public abstract Tuple<bool, string> SessionInitialize(SlaveDevice slave, SlaveSession session);

        public abstract Tuple<bool, string> SessionTerminate(SlaveDevice slave);

        public abstract Tuple<bool, string> RemoteControlDisconnect(SlaveDevice slave);

        public abstract Tuple<bool, string> RemoteControlReconnect(SlaveDevice slave);

        public abstract Tuple<bool, string> SendCommand(SlaveDevice slave, int order , string command);

        public abstract Tuple<bool, string> RejectSlave(SlaveDevice slave);
    }
}
