using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using SharedHost;
using RestSharp;
using SharedHost.Models.Hub;
using Newtonsoft.Json;

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
        Task ReportSessionDisconnected(int slaveID, int ID);
        /// <summary>
        /// Else behind
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        Task ReportSessionReconnected(int slaveID, int ID);
        /// <summary>
        /// Else behind
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        Task ReportSessionTerminated(int slaveID, int ID);
        /// <summary>
        /// Else behind
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        Task ReportSessionInitialized(SlaveDeviceInformation slaveID, int ID);
    }

    public class ClientHub : IClientHub
    {
        private readonly SystemConfig _config;

        private readonly RestClient _NotificationHub;

        public ClientHub(SystemConfig config)
        {
            _config = config;
            _NotificationHub = new RestClient(config.SystemHub+"/Event");
        }


        public async Task ReportSessionDisconnected(int slaveID, int ID)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionDisconnected",
                Message = slaveID.ToString()
            };

            /*generate rest post to signalling server*/
            var request = new RestRequest("Client")
                .AddQueryParameter("ID", ID.ToString())
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }

        public async Task ReportSessionInitialized(SlaveDeviceInformation slaveID, int ID)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionInitialized",
                Message = slaveID.ToString()
            };

            /*generate rest post to signalling server*/
            var request = new RestRequest("Client")
                .AddQueryParameter("ID", ID.ToString())
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }

        public async Task ReportSessionReconnected(int slaveID, int ID)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionReconnected",
                Message = slaveID.ToString()
            };

            /*generate rest post to signalling server*/
            var request = new RestRequest("Client")
                .AddQueryParameter("ID", ID.ToString())
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }

        public async Task ReportSessionTerminated(int slaveID, int ID)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionTerminated",
                Message = slaveID.ToString()
            };

            /*generate rest post to signalling server*/
            var request = new RestRequest("Client")
                .AddQueryParameter("ID", ID.ToString())
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }















        public async Task ReportSlaveObtained(int slaveID)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionTerminated",
                Message = slaveID.ToString()
            };

            /*generate rest post to signalling server*/
            var request = new RestRequest("Broadcast")
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }

        public async Task ReportNewSlaveAvailable(SlaveDeviceInformation device)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionTerminated",
                Message = JsonConvert.SerializeObject(device)
            };

            /*generate rest post to signalling server*/
            var request = new RestRequest("Broadcast")
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }
    }
}