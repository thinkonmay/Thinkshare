using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using SharedHost.Models.Shell;
using WorkerManager.Interfaces;
using System;
using System.Threading.Tasks;

namespace WorkerManager.SlaveDevices.SlaveStates
{
    public class DeviceDisconnected : ISlaveState
    {
        public async Task SessionInitialize(SlaveDevice slave, SlaveSession session)
        {

            return;
        }

        public async Task SessionTerminate(SlaveDevice slave)
        {

            return;
        }

        public async Task RemoteControlDisconnect(SlaveDevice slave)
        {
            return;
        }

        public async Task RemoteControlReconnect(SlaveDevice slave)
        {

            return;
        }

        public async Task RejectSlave(SlaveDevice slave)
        {
            return;
        }

        public string GetSlaveState()
        {
            return SlaveServiceState.Disconnected;
        }

        public async Task InitializeShellSession(SlaveDevice slave, ShellScript script)
        {
            return;
        }

        public async Task TerminateCommandlineSession(SlaveDevice slave, int order)
        {
            return;
        }

        public async Task OnSessionCoreExit(SlaveDevice slave, int SlaveID)
        {
            return;
        }
    }
}
