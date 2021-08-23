using System.Threading.Tasks;
using SlaveManager.Models;
using SharedHost.Models;

namespace SlaveManager.Interfaces
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
        Task ReportSlaveRegistered(SlaveDeviceInformation information);

        /// <summary>
        /// Store slave command line return from agent into database and report to admin via signalR
        /// </summary>
        /// <param name="slaveID"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        Task LogSlaveCommandLine(int slaveID, ReceiveCommand result);

        /// <summary>
        /// Report session core error, store error to database or report to admin
        /// </summary>
        /// <param name="err"> raw absolute time format reported by slave refactorized by admin to store to database</param>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task ReportSessionCoreError(GeneralErrorAbsTime err, int SlaveID);

        /// <summary>
        /// receive error from agent device and save it to database
        /// </summary>
        /// <param name="error"></param>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task ReportAgentError(GeneralErrorAbsTime error, int SlaveID);


        /// <summary>
        /// receive session core process termination from agent or session core module,
        /// in case session core exit is from agent device, no exit report will be recorded to database
        /// </summary>
        /// <param name="slaveID">Slave id of slave device which session core has been terminated</param>
        /// <param name="exit"></param>
        /// <returns></returns>
        Task ReportSessionCoreExit(int slaveID, SessionCoreExitAbsTime exit);

        /// <summary>
        /// Report new session start, invoke from sesssions controller
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="ClientID"></param>
        /// <returns></returns>
        Task ReportNewSession(int SlaveID, int ClientID);

        /// <summary>
        /// report sesssion termination from agent device
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task ReportSessionTermination(Session session);


        Task ReportRemoteControlDisconnected(int SlaveID);
        Task ReportRemoteControlDisconnected(Session session);

        Task ReportRemoteControlReconnect(int SlaveID);
        Task ReportRemoteControlReconnect(Session session);
    }
}