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
using SharedHost.Models.Cluster;

namespace Conductor.Controllers
{
    [ApiController]
    [Route("/RBAC")]
    [Produces("application/json")]
    public class RoleController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly GlobalDbContext _db;

        private readonly IWorkerCommnader _slmsocket;

        private readonly IGlobalStateStore _cache;

        private readonly ILog _log;

        private readonly IClusterRBAC _rbac;


        public RoleController(GlobalDbContext db, 
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





        [Cluster]
        [HttpPost("Grant")]
        public async Task<IActionResult> GrantAccess(string UserName, 
                                                     DateTime start, 
                                                     DateTime end,
                                                     string Description)
        {
            var account = _userManager.FindByNameAsync(UserName);
            var ClusterID = int.Parse((string)HttpContext.Items["ClusterID"]);
            var role = new ClusterRole 
            {
                UserID = account.Id,
                ClusterID = ClusterID,
                Start = start,
                Endtime = end,
                Description = Description
            };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return Ok(role);
        }

        [Cluster]
        [HttpPost("Remove")]
        public async Task<IActionResult> GrantAccess(int RoleID) 
        {
            var ClusterID = int.Parse((string)HttpContext.Items["ClusterID"]);
            var role = _db.Roles.Find(RoleID);
            if (role.ClusterID != ClusterID)
                return Unauthorized();

            role.Endtime = DateTime.Now;
            _db.Roles.Update(role);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
