using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models;
using SlaveManager.Services;
using SlaveManager.SlaveStates;

namespace SlaveManager.Interfaces
{
    public interface ISlaveState
    {
        public Task SessionInitialize(ISlaveDevice slave, SlaveSession session);

        public Task SessionTerminate(ISlaveDevice slave);

        public Task RemoteControlDisconnect(ISlaveDevice slave);

        public Task RemoteControlReconnect(ISlaveDevice slave);

        public Task SendCommand(ISlaveDevice slave, int order, string command);

        public Task RejectSlave(ISlaveDevice slave);

        public string GetSlaveState();
    }
}
