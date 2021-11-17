using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Shell;
using SharedHost.Models.Session;
using System.Threading.Tasks;
using WorkerManager.SlaveDevices;

namespace WorkerManager.Interfaces
{
    public interface ISlaveState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        Task SessionInitialize(SlaveDevice slave, SlaveSession session);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <returns></returns>
        Task SessionTerminate(SlaveDevice slave);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <returns></returns>
        Task RemoteControlDisconnect(SlaveDevice slave);

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <returns></returns>
        Task RemoteControlReconnect(SlaveDevice slave);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        Task InitializeShellSession(SlaveDevice slave, ShellScript order);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <returns></returns>
        Task RejectSlave(SlaveDevice slave);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task OnSessionCoreExit(SlaveDevice slave, int SlaveID);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetSlaveState();
    }
}
