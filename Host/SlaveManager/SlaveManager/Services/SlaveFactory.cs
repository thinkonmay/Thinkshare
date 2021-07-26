using SlaveManager.Interfaces;
using SlaveManager.SlaveStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

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
