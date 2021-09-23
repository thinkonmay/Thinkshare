using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface ISlaveState
    {
        Task SessionInitialize(ISlaveDevice slave, SlaveSession session);

        Task SessionTerminate(ISlaveDevice slave);

        Task RemoteControlDisconnect(ISlaveDevice slave);

        Task RemoteControlReconnect(ISlaveDevice slave);

        Task InitializeShellSession(ISlaveDevice slave, int order);

        Task RejectSlave(ISlaveDevice slave);

        Task OnSessionCoreExit(ISlaveDevice slave, int SlaveID);

        string GetSlaveState();
    }
}
