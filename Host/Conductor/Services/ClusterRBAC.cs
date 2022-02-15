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

        public ClusterRBAC( GlobalDbContext dbContext)
        {
            _db = dbContext;
        }


        public async Task<List<GlobalCluster>> AllowedCluster(int UserID)
        {
            var clusters = new List<GlobalCluster>();

            _db.Roles.Where(x => x.UserID == UserID).ToList()
                .ForEach(x => clusters.Add(x.Cluster));

            _db.Clusters.Where(x => x.OwnerID == UserID).ToList()
                .ForEach(x => clusters.Add(x));

            return clusters;
        }

        public async Task<List<WorkerNode>> AllowedWorker(int UserID)
        {
            var workers = new List<WorkerNode>();
            var clusters = await AllowedCluster(UserID);
            clusters.ForEach(x => x.WorkerNode.ForEach(y => workers.Add(y)));
            return workers;
        }

        public bool IsAllowedCluster(int UserID, int ClusterID)
        {
            var guestRole = _db.Roles.Where(x => 
                                 (x.UserID == UserID) &&
                                 (x.ClusterID == ClusterID) &&
                                 (x.Start < DateTime.Now) &&
                                 (DateTime.Now < x.Endtime)).Any();

            var ownerRole = _db.Clusters.Where(x => x.OwnerID == UserID &&
                                 x.ID == ClusterID).Any();

            return guestRole || ownerRole;
        }

        public bool IsAllowedWorker(int UserID, int WorkerID)
        {
            var worker = _db.Devices.Find(WorkerID);
            var ClusterID = _db.Clusters
                .Where(x => x.WorkerNode.Contains(worker)).First().ID;

            var isAllowed = IsAllowedCluster(UserID,WorkerID);
            var obtained = _db.RemoteSessions.Where(x => x.WorkerID == WorkerID && !x.EndTime.HasValue).Any();

            return isAllowed && !obtained;
        }
    }
}
