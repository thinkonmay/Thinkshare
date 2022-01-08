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
using SharedHost.Auth.ThinkmayAuthProtocol;
using DbSchema.CachedState;

namespace Conductor.Controllers
{
    /// <summary>
    /// Routes used by user to fetch information about the system
    /// </summary>
    [ApiController]
    [Route("/Fetch")]
    [Produces("application/json")]
    public class FetchController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly GlobalDbContext _db;

        private readonly IWorkerCommnader _slmsocket;

        private readonly IGlobalStateStore _cache;

        public FetchController(GlobalDbContext db, 
                            UserManager<UserAccount> userManager,
                            IWorkerCommnader slm,
                            IGlobalStateStore cache)
        {
            _cache = cache;
            _slmsocket = slm;
            _db = db;
            _userManager = userManager;
        }





        [User]
        [HttpGet("Node")]
        public async Task<IActionResult> FetchNode()
        {
            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());
            var publicCluster = _db.Clusters.Where(x => x.Private == false || x.OwnerID == UserID );

            var result = new Dictionary<int,string>();
            foreach (var cluster in publicCluster)
            {
                var snapshoot = await _cache.GetClusterSnapshot(cluster.ID);
                foreach (var state in snapshoot)
                {
                    if (state.Value == WorkerState.Open)
                    {
                        result.TryAdd(state.Key,state.Value);
                    }
                }
            }
            return Ok(result);
        }


        [User]
        [HttpGet("Session")]
        public async Task<IActionResult> GetUserSession()
        {
            var result = new Dictionary<int,string>();
            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());
            var session = _db.RemoteSessions.Where(s => s.ClientId == UserID &&
                                                  !s.EndTime.HasValue).ToList();
            
            Serilog.Log.Information("Fetching session from cache");
            Serilog.Log.Information("User "+UserID+" has "+session.Count().ToString()+" session");

            foreach (var x in session)
            {
                result.TryAdd(x.WorkerID, await _cache.GetWorkerState(x.WorkerID));
            }
            return Ok(result);
        }

        [HttpGet("Worker/Infor")]
        public async Task<IActionResult> GetWorkerInfor(int WorkerID)
        {
            return Ok(await _cache.GetWorkerInfor(WorkerID));
        }
    }
}
