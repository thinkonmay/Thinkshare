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
            Console.WriteLine("device disconnected");
            return;
        }

        public async Task SessionTerminate(ISlaveDevice slave)
        {
            Console.WriteLine("device disconnected");
            return;
        }

        public async Task RemoteControlDisconnect(ISlaveDevice slave)
        {
            Console.WriteLine("device disconnected");
            return;
        }

        public async Task RemoteControlReconnect(ISlaveDevice slave)
        {
            Console.WriteLine("device disconnected");
            return;
        }

        public async Task SendCommand(ISlaveDevice slave, int order, string command)
        {
            Console.WriteLine("device disconnected");
            return;
        }


        public async Task RejectSlave(ISlaveDevice slave)
        {
            Console.WriteLine("device disconnected");
            return;
        }

        public string GetSlaveState()
        {
            return "Device Disconnected";
        }
    }
}
