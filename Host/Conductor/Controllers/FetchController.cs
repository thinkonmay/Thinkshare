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
            var available_node = await _cache.GetWorkerState();
            var systemState = available_node.Where(x => x.Value == WorkerState.Open);
            return Ok();
        }



        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("Session")]
        public async Task<IActionResult> UserGetCurrentSesssion()
        {
            var UserID = HttpContext.Items["UserID"];
            var session = _db.RemoteSessions.Where(s => s.ClientId == Int32.Parse(UserID.ToString()) &&
                                                  !s.EndTime.HasValue).ToList();
            var IDlist = new List<int>();
            session.ForEach(s => IDlist.Add(s.WorkerID));
            var device = (await _cache.GetWorkerState()).Where(d => IDlist.Contains(d.Key) &&  
                                                d.Value == WorkerState.OffRemote && 
                                                d.Value == WorkerState.OnSession);
            
            return Ok(device);
        }
    }
}
