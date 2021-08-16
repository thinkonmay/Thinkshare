using System.Threading.Tasks;
using SlaveManager.Models;
using SharedHost.Models;

namespace SlaveManager.Interfaces
{
    public interface IAdmin
    {
        public Task ReportSlaveRegistered(SlaveDeviceInformation information);

        public Task LogSlaveCommandLine(int slaveID, ReceiveCommand result);

        public Task ReportSessionCoreError(GeneralError err);

        public Task ReportAgentError(GeneralError err);

        public Task ReportSessionCoreExit(int slaveID, SessionCoreExit exit);

        public Task ReportNewSession(int SlaveID, int ClientID);
    }
}