using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Administration;
using SlaveManager.Models;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    public interface IAdminHub
    {
        Task ReportSessionCoreExit(int slaveID, SessionCoreExit exit);

        Task ReportAgentError(GeneralError error);

        Task ReportSessionCoreError(GeneralError error);

        Task ReportSlaveRegistered(SlaveDeviceInformation information);

        Task ReportSessionStart(int SlaveID,int ClientID);

        Task LogSlaveCommandLine(int SlaveID, int ProcessID, string Command);

        Task ReportSessionTermination(int SlaveID, int ClietnID);

    }



    public class AdminHub : Hub<IAdminHub>
    { 

    }
}