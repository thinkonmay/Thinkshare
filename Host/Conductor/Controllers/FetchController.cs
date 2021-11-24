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

        private readonly ApplicationDbContext _db;

        private readonly IWorkerCommnader _slmsocket;

        public FetchController(ApplicationDbContext db, 
                            UserManager<UserAccount> userManager,
                            IWorkerCommnader slm)
        {
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
            var available_node = _db.Devices.Where(o => o.WorkerState == WorkerState.Open);
            return Ok(available_node);
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
            session.ForEach(s => IDlist.Add(s.ID));
            var device = _db.Devices.Where(d => IDlist.Contains(d.ID) &&  
                                                d.WorkerState == WorkerState.OffRemote && 
                                                d.WorkerState == WorkerState.OnSession);
            return Ok(device);
        }
    }
}
