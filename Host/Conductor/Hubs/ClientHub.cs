using Microsoft.Extensions.Options;
using SharedHost.Logging;
using RestSharp;
using SharedHost.Models.Message;
using DbSchema.CachedState;
using System.Threading.Tasks;
using SharedHost;

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
        private readonly ILog _log;


        public ClientHub(IOptions<SystemConfig> config,
                         ILog log,
                         IGlobalStateStore cache)
        {
            _cache = cache;
            _log = log;
            _NotificationHub = new RestClient($"{config.Value.SystemHub}/User/Event");
        }


        public async Task ReportSessionDisconnected(int slaveID, int ID)
        {
            var data = new EventModel
            {
                EventName = "ReportSessionDisconnected",
                Message = slaveID.ToString()
            };

            _log.Information("Sending session disconnected event to client "+ID.ToString());

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

            _log.Information("Sending session initialized event to client "+ID.ToString());

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

            _log.Information("Sending session reconnected event to client "+ID.ToString());

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

            _log.Information("Sending session reconnected event to client "+ID.ToString());

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

            _log.Information("Broadcasting worker obtained event to all client");

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

            _log.Information("Broadcasting worker "+WorkerID.ToString()+" available event to all client");

            /*generate rest post to signalling server*/
            var request = new RestRequest("Broadcast")
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }
    }
}