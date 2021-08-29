using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SharedHost.Models;
using Conductor.Administration;
using Conductor.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SharedHost.Models.Device;
using SharedHost.Models.Error;

namespace SignalRChat.Hubs
{
    public interface IAdminHub
    {
        Task ReportAgentError(GeneralError error);

        Task ReportSessionCoreError(GeneralError error);

        Task ReportSlaveRegistered(SlaveDeviceInformation information);

        Task ReportSessionStart(int SlaveID,int ClientID);

        Task LogSlaveCommandLine(int SlaveID, int ProcessID, string Command);

        Task ReportSessionTermination(int SlaveID, int ClietnID);
    }


    [Authorize(Roles = "SystemManager")]
    public class AdminHub : Hub<IAdminHub>
    {
        
    }
}