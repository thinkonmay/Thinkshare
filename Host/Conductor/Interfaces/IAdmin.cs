using System.Threading.Tasks;
using Conductor.Models;
using SharedHost.Models;
using SharedHost.Models.Command;
using SharedHost.Models.Device;
using SharedHost.Models.Error;
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
        Task<bool> ReportSlaveRegistered(SlaveDeviceInformation information);



        /// <summary>
        /// Report new slave registration to the system
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task EndAllRemoteSession(int SlaveID);

        /// <summary>
        /// Report new slave registration to the system
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task EndAllShellSession(int SlaveID);

        /// <summary>
        /// Store slave command line return from agent into database and report to admin via signalR
        /// </summary>
        /// <returns></returns>
        Task ReportShellSessionTerminated(int SlaveID,int ProcessID);


        /// <summary>
        /// Store slave command line return from agent into database and report to admin via signalR
        /// </summary>
        /// <returns></returns>
        Task LogSlaveCommandLine(ReceiveCommand command);

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


        Task ReportRemoteControlDisconnected(int SlaveID);

        Task ReportRemoteControlDisconnectedFromSignalling(SessionPair session);

        Task ReportRemoteControlDisconnected(RemoteSession session);

        Task ReportRemoteControlReconnect(int SlaveID);

        Task ReportRemoteControlReconnect(RemoteSession session);
    }
}