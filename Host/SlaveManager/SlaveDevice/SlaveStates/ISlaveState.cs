using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Shell;
using SharedHost.Models.Session;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface ISlaveState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        Task SessionInitialize(ISlaveDevice slave, SlaveSession session);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <returns></returns>
        Task SessionTerminate(ISlaveDevice slave);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <returns></returns>
        Task RemoteControlDisconnect(ISlaveDevice slave);

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <returns></returns>
        Task RemoteControlReconnect(ISlaveDevice slave);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        Task InitializeShellSession(ISlaveDevice slave, ShellScript order);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <returns></returns>
        Task RejectSlave(ISlaveDevice slave);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task OnSessionCoreExit(ISlaveDevice slave, int SlaveID);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetSlaveState();
    }
}
