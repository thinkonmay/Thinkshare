using Microsoft.AspNetCore.Mvc;
using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
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
        [HttpPost("Role")]
        public async Task<IActionResult> GrantAccess([FromBody] ClusterRoleRequest request)
        {
            var ClusterID = Int32.Parse(HttpContext.Items["ClusterID"].ToString());
            var account = _userManager.FindByNameAsync(request.User);
            if(account == null)
                return BadRequest("Cannot find this account");

            var role = new ClusterRole 
            {
                UserID = account.Id,
                ClusterID = ClusterID,
                Start = request.Start.HasValue ? request.Start : DateTime.Now,
                Endtime = request.Endtime.HasValue ? request.Endtime : DateTime.Now.AddMonths(1),
                Description = request.Description
            };

            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return Ok(role);
        }

        [Cluster]
        [HttpDelete("Role")]
        public async Task<IActionResult> DeleteAccess(int RoleID) 
        {
            var ClusterID = Int32.Parse(HttpContext.Items["ClusterID"].ToString());
            var role = _db.Roles.Find(RoleID);
            if (role.ClusterID != ClusterID)
                return Unauthorized();

            role.Endtime = DateTime.Now;
            _db.Roles.Update(role);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Cluster]
        [HttpGet("Role")]
        public async Task<IActionResult> CurrentAccess() 
        {
            var ClusterID = Int32.Parse(HttpContext.Items["ClusterID"].ToString());
            var roles = _db.Roles.Where(x => (x.ClusterID == ClusterID) &&
                                             (DateTime.Now < x.Endtime));
            return Ok(roles);
        }
    }
}
