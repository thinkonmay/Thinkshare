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
using SharedHost.Auth;
using DbSchema.CachedState;
using SharedHost.Logging;

namespace Conductor.Controllers
{
    [ApiController]
    [Route("/Fetch")]
    [Produces("application/json")]
    public class FetchController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly GlobalDbContext _db;

        private readonly IWorkerCommnader _slmsocket;

        private readonly IGlobalStateStore _cache;

        private readonly ILog _log;

        private readonly IClusterRBAC _rbac;


        public FetchController(GlobalDbContext db, 
                            UserManager<UserAccount> userManager,
                            ILog log,
                            IClusterRBAC rbac,
                            IWorkerCommnader slm,
                            IGlobalStateStore cache)
        {
            _db = db;
            _log = log;
            _rbac = rbac;
            _cache = cache;
            _slmsocket = slm;
            _userManager = userManager;
        }





        [User]
        [HttpGet("Node")]
        public async Task<IActionResult> FetchNode()
        {
            var result = new Dictionary<int,string>();
            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());
            var allowedWorkers = await _rbac.AllowedWorker(UserID); 
            allowedWorkers.ForEach(async x => {
                if(await _cache.GetWorkerState(x.ID) == WorkerState.Open)
                {
                    result.Add(x.ID,WorkerState.Open);
                }});
            return Ok(result);
        }


        [User]
        [HttpGet("Session")]
        public async Task<IActionResult> GetUserSession()
        {
            var result = new Dictionary<int,string>();
            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());
            var session = _db.RemoteSessions.Where(s => (s.ClientId == UserID) &&
                                                        (s.StartTime.HasValue) &&
                                                       !(s.EndTime.HasValue)).ToList();
            
            session.ForEach(async x => {
                result.TryAdd(x.WorkerID, await _cache.GetWorkerState(x.WorkerID));
                });
            return Ok(result);
        }

        [HttpGet("Worker/Infor")]
        public async Task<IActionResult> GetWorkerInfor(int WorkerID)
        {
            return Ok(await _cache.GetWorkerInfor(WorkerID));
        }
    }
}
