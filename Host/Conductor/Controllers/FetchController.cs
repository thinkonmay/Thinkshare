using Microsoft.AspNetCore.Mvc;
using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using Microsoft.Extensions.Caching.Distributed;
using SharedHost.Auth.ThinkmayAuthProtocol;
using DbSchema.CachedState;
using System.Linq;

namespace Conductor.Controllers
{
    /// <summary>
    /// Routes used by user to fetch information about the system
    /// </summary>
    [User]
    [ApiController]
    [Route("/Fetch")]
    [Produces("application/json")]
    public class FetchController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly GlobalDbContext _db;

        private readonly IWorkerCommnader _slmsocket;

        private readonly GlobalStateStore _cache;

        public FetchController(GlobalDbContext db, 
                            UserManager<UserAccount> userManager,
                            IWorkerCommnader slm,
                            GlobalStateStore cache)
        {
            _cache = cache;
            _slmsocket = slm;
            _db = db;
            _userManager = userManager;
        }





        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("Node")]
        public async Task<IActionResult> FetchNode()
        {
            var publicCluster = _db.Clusters.Where(x => x.Private == false);

            var result = new List<WorkerNode>();
            foreach (var cluster in publicCluster)
            {
                var snapshoot = await _cache.GetClusterSnapshot(cluster.ID);
                foreach (var state in snapshoot)
                {
                    if (state.Value == WorkerState.Open)
                    {
                        var node = await _cache.GetWorkerInfor(state.Key);
                        if(node == null)
                        {
                            node = _db.Devices.Find(state.Key);
                            await _cache.CacheWorkerInfor(node);
                        }
                        result.Add(node);
                    }
                }
            }
            return Ok(result);
        }



        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("Session")]
        public async Task<IActionResult> UserGetCurrentSesssion()
        {
            var UserID = HttpContext.Items["UserID"];
            var nodeList = new List<int>();
            var session = _db.RemoteSessions.Where(s => s.ClientId == Int32.Parse(UserID.ToString()) &&
                                                  !s.EndTime.HasValue).ToList();
            
            session.ForEach(x => nodeList.Add(x.ClientId));
            var result = new List<WorkerNode>();

            var publicCluster = _db.Clusters.Where(x => x.Private == false);

            foreach (var cluster in publicCluster)
            {
                var snapshoot = await _cache.GetClusterSnapshot(cluster.ID);
                foreach (var state in snapshoot)
                {
                    if (state.Value == WorkerState.OffRemote &&
                        state.Value == WorkerState.OnSession &&
                        nodeList.Contains(state.Key))
                    {
                        var node = await _cache.GetWorkerInfor(state.Key);
                        if(node == null)
                        {
                            node = _db.Devices.Find(state.Key);
                            await _cache.CacheWorkerInfor(node);
                        }
                        node.WorkerState = state.Value;
                        result.Add(node);
                    }
                }
            }

            
            return Ok(result);
        }
    }
}
