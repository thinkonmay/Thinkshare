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

namespace SignalRChat.Hubs
{
    public interface IAdminHub
    {
        Task ReportSlaveRegistered(SlaveDeviceInformation information);

        Task ReportSessionStart(int SlaveID, UserAccount Client);

        Task LogSlaveCommandLine(int SlaveID, int ProcessID, string Command);

        Task ReportSessionTermination(int SlaveID, UserAccount Client );
    }

    [Authorize]
    public class AdminHub : Hub<IAdminHub> {   }
}