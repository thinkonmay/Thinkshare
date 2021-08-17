using System.Threading.Tasks;
using SlaveManager.Models;
using SharedHost.Models;

namespace SlaveManager.Interfaces
{
    public interface IAdmin
    {
        Task ReportSlaveRegistered(SlaveDeviceInformation information);

        Task LogSlaveCommandLine(int slaveID, ReceiveCommand result);

        Task ReportSessionCoreError(GeneralErrorAbsTime err, int SlaveID);

        Task ReportAgentError(GeneralErrorAbsTime error, int SlaveID);

        Task ReportSessionCoreExit(int slaveID, SessionCoreExitAbsTime exit);

        Task ReportNewSession(int SlaveID, int ClientID);

        Task ReportSessionTermination(int SlaveID, int ClientID);
    }
}