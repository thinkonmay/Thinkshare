using System.Linq;
using System;
using System.Threading.Tasks;
using RestSharp;
using Conductor.Interfaces;
using SharedHost;
using DbSchema.SystemDb.Data;
using Microsoft.Extensions.Options;
using DbSchema.CachedState;
using System.Collections.Generic;
using SharedHost.Models.Cluster;
using SharedHost.Models.Device;

namespace Conductor.Services
{
    public class ClusterRBAC : IClusterRBAC
    {
        private readonly GlobalDbContext _db;

        private readonly IGlobalStateStore _cache;

        public ClusterRBAC(IOptions<SystemConfig> config, 
                            GlobalDbContext dbContext, 
                            IGlobalStateStore cache)
        {
            _db = dbContext;
        }


        public async Task<List<GlobalCluster>> AllowedCluster(int UserID)
        {
            var clusters = new List<GlobalCluster>();
            _db.Roles.Where(x => x.UserID == UserID).ToList()
                .ForEach(x => clusters.Add(x.Cluster));
            return clusters;
        }

        public async Task<List<WorkerNode>> AllowedWorker(int UserID)
        {
            var workers = new List<WorkerNode>();
            var clusters = await this.AllowedCluster(UserID);
            clusters.ForEach(x => x.WorkerNode.ForEach(y => workers.Add(y)));

            workers.ForEach(x => Task.Run(async () => {
                var isOpen = (await _cache.GetWorkerState(x.ID)) == WorkerState.Open;
                var obtained = _db.RemoteSessions.Where(x => x.WorkerID == x.ID && !x.EndTime.HasValue).Any();
                if(!isOpen || obtained) { workers.Remove(x); }
            }).Wait());
           
            return workers;
        }

        public async Task<bool> IsAllowedWorker(int UserID, int ClusterID)
        {
            var guestRole = _db.Roles.Where(x => 
                                 (x.UserID == UserID) &&
                                 (x.ClusterID == ClusterID) &&
                                 (x.Start < DateTime.Now) &&
                                 (DateTime.Now < x.Endtime));

            var ownerRole = _db.Clusters.Where(x => x.OwnerID == UserID &&
                                 x.ID == ClusterID);

            return ((guestRole != null) || (ownerRole != null));
        }

        public async Task<bool> IsAllowedCluster(int UserID, int WorkerID)
        {
            var worker = _db.Devices.Find(WorkerID);
            var ClusterID = _db.Clusters
                .Where(x => x.WorkerNode.Contains(worker)).First().ID;

            var guestRole = _db.Roles.Where(x => 
                                 (x.UserID == UserID) &&
                                 (x.ClusterID == ClusterID) &&
                                 (x.Start < DateTime.Now) &&
                                 (DateTime.Now < x.Endtime));

            var ownerRole = _db.Clusters.Where(x => x.OwnerID == UserID &&
                                 x.ID == ClusterID);


            var isOpen = (await _cache.GetWorkerState(WorkerID)) == WorkerState.Open;
            var obtained = _db.RemoteSessions.Where(x => x.WorkerID == WorkerID && !x.EndTime.HasValue).Any();

            return (((guestRole != null) || (ownerRole != null)) && isOpen && !obtained);
        }
    }
}
