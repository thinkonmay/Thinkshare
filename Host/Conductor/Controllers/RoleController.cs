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
        private readonly GlobalDbContext _db;

        private readonly IGlobalStateStore _cache;

        private readonly ILog _log;

        private readonly IClusterRBAC _rbac;


        public RoleController(GlobalDbContext db, 
                            ILog log,
                            IClusterRBAC rbac,
                            IGlobalStateStore cache)
        {
            _db = db;
            _log = log;
            _rbac = rbac;
            _cache = cache;
        }





        [Cluster]
        [HttpPost("Role")]
        public async Task<IActionResult> GrantAccess([FromBody] ClusterRoleRequest request)
        {
            var ClusterID = Int32.Parse(HttpContext.Items["ClusterID"].ToString());
            var account = _db.Users.Where(x => x.UserName == request.User).First();
            if(account == null)
                return BadRequest("Cannot find this account");

            var role = new ClusterRole 
            {
                UserID = account.Id,
                ClusterID = ClusterID,
                Start = request.Start.HasValue ? request.Start : DateTime.Now,
                Endtime = request.Endtime.HasValue ? request.Endtime : DateTime.Now.AddMinutes(15),
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
