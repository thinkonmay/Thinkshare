using Microsoft.AspNetCore.Mvc;
using DbSchema.SystemDb.Data;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost.Models.Cluster;
using Authenticator.Interfaces;
using SharedHost;
using RestSharp;
using System.Linq;
using DbSchema.CachedState;

namespace Authenticator.Controllers
{
    /// <summary>
    /// Routes used by user to fetch information about the system
    /// </summary>
    [Manager]
    [ApiController]
    [Route("/Cluster")]
    [Produces("application/json")]
    public class ClusterController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly GlobalDbContext _db;

        private readonly ITokenGenerator _token;

        private readonly IGlobalStateStore _cache;

        public ClusterController(GlobalDbContext db,
                                 UserManager<UserAccount> userManager,
                                 ITokenGenerator token,
                                 IGlobalStateStore cache)
        {
            _cache = cache;
            _db = db;
            _token = token;
            _userManager = userManager;
        }





        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [Manager]
        [HttpPost("Register")]
        public async Task<IActionResult> NewCluster(string ClusterName, bool Private)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account =  await _userManager.FindByIdAsync((string)ManagerID);
            if(account.ManagedCluster.Where(x => x.Name == ClusterName).Any())
            {
                return BadRequest("Choose a different name");
            }

            var cluster = new GlobalCluster
            {
                Name = ClusterName,
                Register = DateTime.Now,
                Private = Private
            };
            account.ManagedCluster.Add(cluster);
            await _userManager.UpdateAsync(account);

            return Ok();
        }

        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [Manager]
        [HttpGet("Token")]
        public async Task<IActionResult> GetToken(string ClusterName)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account = await _userManager.FindByIdAsync((string)ManagerID);
            var cluster = account.ManagedCluster.Where(x => x.Name == ClusterName);
            if(!cluster.Any())
            {
                return BadRequest("Cluster not found");
            }

            var token = await _token.GenerateClusterJwt((string)ManagerID,ClusterName,cluster.First().ID);
            return Ok(token);
        }

        [Manager]
        [HttpPost("Worker/Register")]
        public async Task<IActionResult> Register(string ClusterName, [FromBody] WorkerRegisterModel body)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account = await _userManager.FindByIdAsync((string)ManagerID);
            
            var cluster = account.ManagedCluster.Where(x => x.Name == ClusterName).FirstOrDefault();
            var newWorker = new WorkerNode
            {
                Register = DateTime.Now,
                CPU = body.CPU,
                GPU = body.GPU,
                RAMcapacity = body.RAMcapacity,
                OS = body.OS
            };
            cluster.WorkerNode.Add(newWorker);
            _db.Clusters.Update(cluster);
            await _db.SaveChangesAsync();

            // get GlobalID and return to cluster
            var result = new IDAssign
            {
                GlobalID = newWorker.ID,
                ClusterID = cluster.ID,
            };

            await _cache.CacheWorkerInfor(newWorker);
            return Ok(result);
        }

        [Manager]
        [HttpGet("Infor")]
        public async Task<IActionResult> getInfor(string ClusterName)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account = await _userManager.FindByIdAsync((string)ManagerID);
            var cluster = account.ManagedCluster.Where(x => x.Name == ClusterName);
            if (!cluster.Any())
            {
                return BadRequest("Cluster not found");
            }
            else
            {
                return Ok(cluster.First());
            }
        }
    }
}
