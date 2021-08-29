using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
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

        public async Task SendCommand(ISlaveDevice slave, ForwardCommand command)
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

        public async Task InitializeCommandlineSession(ISlaveDevice slave, int order)
        {
            return;
        }

        public async Task TerminateCommandlineSession(ISlaveDevice slave, int order)
        {
            return;
        }

        public async Task OnSessionCoreExit(ISlaveDevice slave, int SlaveID)
        {
            return;
        }
    }
}
