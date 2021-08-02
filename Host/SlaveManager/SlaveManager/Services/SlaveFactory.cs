using SlaveManager.Interfaces;
using SlaveManager.SlaveDevices;
using SlaveManager.SlaveDevices.SlaveStates;
using System.Net.WebSockets;

namespace SlaveManager.Services
{
    public interface ISlaveFactory
    {
        public SlaveDevice CreateSlaveDevice(WebSocket conn, bool connected);
    }

    public class SlaveFactory : ISlaveFactory
    {
        ISlaveConnection _connection;

        public SlaveFactory(ISlaveConnection connection)
        {
            _connection = connection;
        }

        public SlaveDevice CreateSlaveDevice(WebSocket conn, bool connected)
        {
            ISlaveState state;
            if (connected)
            {
                state = new DeviceOpen();
            }
            else
            {
                state = new DeviceDisconnected();
            }
            return new SlaveDevice(conn, state, _connection);
        }

    }
}
