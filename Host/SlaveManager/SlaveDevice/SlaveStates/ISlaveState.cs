using SharedHost.Models;
using SlaveManager.SlaveDevices;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface ISlaveState
    {
        Task SessionInitialize(ISlaveDevice slave, SlaveSession session);

        Task SessionTerminate(ISlaveDevice slave);

        Task RemoteControlDisconnect(ISlaveDevice slave);

        Task RemoteControlReconnect(ISlaveDevice slave);

        Task SendCommand(ISlaveDevice slave, int order, string command);

        Task RejectSlave(ISlaveDevice slave);

        string GetSlaveState();
    }
}
