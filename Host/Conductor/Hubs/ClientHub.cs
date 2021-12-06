using Microsoft.AspNetCore.SignalR;
using System.Linq;
using DbSchema.SystemDb.Data;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using SharedHost;
using RestSharp;
using SharedHost.Models.Hub;
using DbSchema.CachedState;
using Newtonsoft.Json;

namespace Conductor.Hubs
{
    public interface IClientHub
    {
        /// <summary>
        /// When slave device not use by current user, but obtained by someone => trigger and noti for all user connect this hub (ke ca someone).
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        Task ReportSlaveObtained(int slaveID);

        Task ReportNewSlaveAvailable(int WorkerID);

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

        Task ReportSessionTerminated(int WorkerID, int ID);

        Task ReportSessionInitialized(int WorkerID, int ID);
    }

    public class ClientHub : IClientHub
    {
        private readonly RestClient _NotificationHub;
        private readonly IGlobalStateStore _cache;

        public ClientHub(IOptions<SystemConfig> config,
                         IGlobalStateStore cache)
        {
            _cache = cache;
            _NotificationHub = new RestClient(config.Value.SystemHub+"/User/Event");
        }


        public async Task ReportSessionDisconnected(int slaveID, int ID)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionDisconnected",
                Message = slaveID.ToString()
            };

            Serilog.Log.Information("Sending session disconnected event to client "+ID.ToString());

            /*generate rest post to signalling server*/
            var request = new RestRequest("Client")
                .AddQueryParameter("ID", ID.ToString())
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }

        public async Task ReportSessionInitialized(int WorkerID, int ID)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionOn",
                Message = WorkerID.ToString()
            };

            Serilog.Log.Information("Sending session initialized event to client "+ID.ToString());

            /*generate rest post to signalling server*/
            var request = new RestRequest("Client")
                .AddQueryParameter("ID", ID.ToString())
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }

        public async Task ReportSessionReconnected(int WorkerID, int ID)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionReconnected",
                Message = WorkerID.ToString()
            };

            Serilog.Log.Information("Sending session reconnected event to client "+ID.ToString());

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

            Serilog.Log.Information("Sending session reconnected event to client "+ID.ToString());

            /*generate rest post to signalling server*/
            var request = new RestRequest("Client")
                .AddQueryParameter("ID", ID.ToString())
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }















        public async Task ReportSlaveObtained(int WorkerID)
        {
            var data = new EventModel
            {
                EventName = "ReportSlaveObtained",
                Message = WorkerID.ToString()
            };

            Serilog.Log.Information("Broadcasting worker obtained event to all client");

            /*generate rest post to signalling server*/
            var request = new RestRequest("Broadcast")
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }


        public async Task ReportNewSlaveAvailable(int WorkerID)
        {
            var data = new EventModel
            {
                EventName = "ReportNewSlaveAvailable",
                Message = WorkerID.ToString()
            };

            Serilog.Log.Information("Broadcasting worker "+WorkerID.ToString()+" available event to all client");

            /*generate rest post to signalling server*/
            var request = new RestRequest("Broadcast")
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }
    }
}