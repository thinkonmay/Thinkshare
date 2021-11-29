﻿using SharedHost.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using RestSharp;
using Newtonsoft;
using SharedHost.Models.Shell;
using Newtonsoft.Json;
using System.Net;
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

        public WorkerCommander(IOptions<SystemConfig> config, GlobalDbContext dbContext, IGlobalStateStore cache)
        {
            _db = dbContext;
            _cache = cache;
            _Cluster =  new RestClient(config.Value.SystemHub + "/Cluster");
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

        public async Task SessionReconnect(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Reconnect")
                .AddQueryParameter("SlaveID", SlaveID.ToString());
            request.Method = Method.POST;

            await _Cluster.ExecuteAsync(request);
        }

        public async Task SessionDisconnect(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Disconnect")
                .AddQueryParameter("SlaveID", SlaveID.ToString());
            request.Method = Method.POST;

            await _Cluster.ExecuteAsync(request);
        }

        public async Task SessionInitialize(int ID, string token)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Initialize")
                .AddQueryParameter("token", token);
            request.Method = Method.POST;

            await _Cluster.ExecuteAsync(request);
        }

        public async Task SessionTerminate(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Terminate")
                .AddQueryParameter("SlaveID", SlaveID.ToString());
            request.Method = Method.POST;

            await _Cluster.ExecuteAsync(request);
        }


        public async Task AssignGlobalID(int ClusterID ,int GlobalID, int PrivateID)
        {
            var request = new RestRequest("GrantID")
                .AddQueryParameter("ClusterID", ClusterID.ToString())
                .AddQueryParameter("GlobalID", GlobalID.ToString())
                .AddQueryParameter("PrivateID", PrivateID.ToString());
            request.Method = Method.POST;

            await _Cluster.ExecuteAsync(request);

        }
    }
}
