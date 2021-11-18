using System.Threading.Tasks;
using SharedHost.Models.Shell;
using SharedHost.Models.Device;
using SharedHost.Models.Session;

namespace Conductor.Interfaces
{
    /// <summary>
    /// Admin is an singleton object responsible for handle event related to system management 
    /// , included : report slave error, report session core terminattion, report new registered slave device....
    /// and redirect those event to be  save in database or report to admin (connected to admin hub)
    /// /// </summary>
    public interface IAdmin
    {
        /// <summary>
        /// Report new slave registration to the system
        /// </summary>
        /// <param name="information"></param>
        /// <returns></returns>
        Task<bool> ReportSlaveRegistered(WorkerNode information);



        /// <summary>
        /// Report new slave registration to the system
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task EndAllRemoteSession(int SlaveID);



        /// <summary>
        /// Report new session start, invoke from sesssions controller
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task ReportNewSession(RemoteSession session);

        /// <summary>
        /// report sesssion termination from agent device
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task ReportSessionTermination(RemoteSession session);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task ReportRemoteControlDisconnected(int SlaveID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task ReportRemoteControlDisconnectedFromSignalling(SessionAccession session);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task ReportRemoteControlDisconnected(RemoteSession session);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task ReportRemoteControlReconnect(int SlaveID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task ReportRemoteControlReconnect(RemoteSession session);
    }
}