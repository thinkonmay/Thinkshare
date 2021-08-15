using SharedHost.Models;
using SlaveManager.Interfaces;
using System;
using System.Threading.Tasks;

namespace SlaveManager.SlaveDevices.SlaveStates
{
    public class DeviceDisconnected : ISlaveState
    {
        public async Task SessionInitialize(ISlaveDevice slave, SlaveSession session)
        {

            return;
        }

        public async Task SessionTerminate(ISlaveDevice slave)
        {

            return;
        }

        public async Task RemoteControlDisconnect(ISlaveDevice slave)
        {

            return;
        }

        public async Task RemoteControlReconnect(ISlaveDevice slave)
        {

            return;
        }

        public async Task SendCommand(ISlaveDevice slave, int order, string command)
        {
            return;
        }


        public async Task RejectSlave(ISlaveDevice slave)
        {

            return;
        }

        public string GetSlaveState()
        {
            return SlaveServiceState.Disconnected;
        }
    }
}
