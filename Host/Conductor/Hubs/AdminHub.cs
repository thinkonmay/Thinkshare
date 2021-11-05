using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SharedHost.Models;
using Conductor.Services;
using Conductor.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SharedHost.Models.Device;
using SharedHost.Models.Error;
using SharedHost.Models.User;
using SharedHost.Models.Shell;
using SharedHost.Auth.ThinkmayAuthProtocol;

namespace SignalRChat.Hubs
{
    public interface IAdminHub
    {
        Task ReportSlaveRegistered(SlaveDeviceInformation information);

        Task LogShellOutput(ShellOutput output);
    }

    [Admin]
    public class AdminHub : Hub<IAdminHub> {   }
}