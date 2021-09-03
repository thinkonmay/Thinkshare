using System.Threading.Tasks;
using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Error;

namespace SlaveManager.Interfaces
{
    /// <summary>
    /// Admin is an singleton object responsible for handle event related to system management 
    /// , included : report slave error, report session core terminattion, report new registered slave device....
    /// and redirect those event to be  save in database or report to admin (connected to admin hub)
    /// /// </summary>
    public interface IConductorSocket
    {
        /// <summary>
        /// Report new slave registration to the system
        /// </summary>
        /// <param name="information"></param>
        /// <returns></returns>
        Task<bool> ReportSlaveRegistered(SlaveDeviceInformation information);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task ReportSlaveDisconnected(int SlaveID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task ReportShellSessionTerminated(ForwardCommand command);

        /// <summary>
        /// Store slave command line return from agent into database and report to admin via signalR
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        Task LogSlaveCommandLine(ReceiveCommand result);

        /// <summary>
        /// Report session core error, log error to database or report to admin
        /// </summary>
        /// <param name="err"> raw absolute time format reported by slave refactorized by admin to store to database</param>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task ReportError (ReportedError err);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task ReportRemoteControlDisconnected(int SlaveID);
    }
}