﻿using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SharedHost.Models;
using Conductor.Administration;
using Conductor.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SharedHost.Models.Device;
using SharedHost.Models.Error;
using SharedHost.Models.User;
using SharedHost.Models.Command;

namespace SignalRChat.Hubs
{
    public interface IAdminHub
    {
        Task ReportSlaveRegistered(SlaveDeviceInformation information);

        Task LogShellOutput(ShellOutput output);
    }

    [Authorize]
    public class AdminHub : Hub<IAdminHub> {   }
}