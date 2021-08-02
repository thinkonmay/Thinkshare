using SharedHost.Models;
using SlaveManager.SlaveDevices;
using System.Threading.Tasks;

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
