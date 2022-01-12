using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Conductor.Interfaces;
using SharedHost;
using DbSchema.SystemDb.Data;
using Microsoft.Extensions.Options;
using DbSchema.CachedState;

namespace Conductor.Services
{
    public class WorkerCommander : IWorkerCommnader
    {
        private readonly RestClient _Cluster;

        private readonly GlobalDbContext _db;


        private readonly IGlobalStateStore _cache;

        public WorkerCommander(IOptions<SystemConfig> config, 
                                GlobalDbContext dbContext, 
                                IGlobalStateStore cache)
        {
            _db = dbContext;
            _cache = cache;
            _Cluster =  new RestClient(config.Value.SystemHub + "/Cluster/Worker");
        }


        public async Task<string> GetWorkerState(int WorkerID)
        {
            var publicCluster = _db.Clusters.ToList();

            foreach (var cluster in publicCluster)
            {
                var snapshoot = await _cache.GetClusterSnapshot(cluster.ID);
                foreach (var state in snapshoot)
                {
                    if (state.Key == WorkerID)
                    {
                        return state.Value;
                    }
                }
            }

            return null;
        }

        public async Task SessionReconnect(int WorkerID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Reconnect")
                .AddQueryParameter("WorkerID", WorkerID.ToString());
            request.Method = Method.POST;

            await _Cluster.ExecuteAsync(request);
        }

        public async Task SessionDisconnect(int WorkerID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Disconnect")
                .AddQueryParameter("WorkerID", WorkerID.ToString());
            request.Method = Method.POST;

            await _Cluster.ExecuteAsync(request);
        }

        public async Task SessionInitialize(int WorkerID, string token)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Initialize")
                .AddQueryParameter("token", token)
                .AddQueryParameter("WorkerID", WorkerID.ToString());
            request.Method = Method.POST;

            await _Cluster.ExecuteAsync(request);
        }

        public async Task SessionTerminate(int WorkerID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Terminate")
                .AddQueryParameter("WorkerID", WorkerID.ToString());
            request.Method = Method.POST;

            await _Cluster.ExecuteAsync(request);
        }
    }
}
