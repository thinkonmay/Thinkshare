using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SharedHost.Models;
using Conductor.Services;
using Conductor.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SharedHost.Models.Device;
using Conductor.Interfaces;
using SharedHost.Models.Session;
using Conductor.Services;

namespace SignalRChat.Hubs
{
    public interface IClientHub
    {
        /// <summary>
        /// When slave device not use by current user, but obtained by someone => trigger and noti for all user connect this hub (ke ca someone).
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        Task ReportSlaveObtained(int slaveID);

        /// <summary>
        /// Slave device end session with another user => trigger this func => noti new slave available for all users
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        Task ReportNewSlaveAvailable(SlaveDeviceInformation device);
        /// <summary>
        /// Disconnected by something wrong on server => report to user use this device
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        Task ReportSessionDisconnected(int slaveID);
        /// <summary>
        /// Else behind
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        Task ReportSessionReconnected(int slaveID);
        /// <summary>
        /// Else behind
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        Task ReportSessionTerminated(int slaveID);
        /// <summary>
        /// Else behind
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        Task ReportSessionInitialized(SlaveDeviceInformation slaveID);
    }

    [Authorize]
    public class ClientHub : Hub<IClientHub>
    {
        private readonly ITokenGenerator _token;

        public ClientHub(ITokenGenerator token)
        {
            _token = token;
        }
        
        public override Task OnConnectedAsync()
        {
            int UserID = _token.GetUserFromHttpRequest(Context.User);
            Groups.AddToGroupAsync(Context.ConnectionId,UserID.ToString());

            return base.OnConnectedAsync();
        }
    }
}