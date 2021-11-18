using SharedHost.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using SharedHost.Models.Shell;
using RestSharp;

namespace Conductor.Interfaces
{
    public interface ISlaveManagerSocket
    {

        Task<bool> SearchForSlaveID(int slave_id);






        /// <summary>
        /// Send slave reject signal to slave device without delete slaveid from slavepool
        /// </summary>
        /// <param name="slaveid"></param>
        /// <returns></returns>
        Task<bool> DisconnectSlave(int slaveid);

        /// <summary>
        /// Send reject slave message to agent, then remove slavedevice and slave id from slavepool
        /// </summary>
        /// <param name="slaveid"></param>
        /// <returns></returns>
        Task<bool> RejectSlave(int slaveid);













        Task<bool> RemoteControlReconnect(int slaveid);

        Task<bool> RemoteControlDisconnect(int slaveid);

        Task<bool> SessionInitialize(int ID, string token, SessionBase session);

        Task<bool> SessionTerminate(int slaveid);
    }
}
