using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using SharedHost.Models.Cluster;
using SharedHost;
using Microsoft.Extensions.Options;
using RestSharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Authenticator.Interfaces;
using SharedHost.Models.User;
using System;
using SharedHost.Models.AWS;
using SharedHost.Models.Device;
using System.Threading.Tasks;
using DbSchema.DbSeeding;
using SharedHost.Auth;
using DbSchema.SystemDb.Data;
using SharedHost.Logging;
using System.Text;

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("/Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly GlobalDbContext _db;
        private readonly SystemConfig _config;
        private readonly ITokenGenerator _token;
        private readonly ILog _log;

        public ManagerController(UserManager<UserAccount> userManager,
                                 ITokenGenerator token,
                                 ILog log,
                                 IOptions<SystemConfig> config,
                                 GlobalDbContext db)
        {
            _userManager = userManager;
            _db = db;
            _log = log;
            _token = token;
            _config = config.Value;
        }



        [User]
        [HttpPost("Request")]
        public async Task<IActionResult> Elevate(string Description)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            
            var result = await _userManager.AddToRoleAsync(account,RoleSeeding.MOD);
            _log.Information(UserID.ToString() + " want to elevate to manager, Description: "+Description);
            return Ok(result);
        }

        [Manager]
        [HttpPost("ManagedCluster/Request")]
        public async Task<IActionResult> RequestCluster( string ClusterName)
        {
            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());
            if(_db.Clusters.Where(x => x.Name == ClusterName && x.OwnerID == Int32.Parse(UserID.ToString()) ).Any())
            {
                return BadRequest("Choose a different name");
            }

            var request = new RestRequest(_config.AutoScaling+"/Instance/Managed");
            request.Method = Method.GET;

            var client = new RestClient();
            client.Timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
            var coturnResult = await client.ExecuteAsync(request);
            var instance = JsonConvert.DeserializeObject<ClusterInstance>(coturnResult.Content);
            if(coturnResult == null)
            {
                return BadRequest("Fail to intialize cluster");
            }

            var cluster = new GlobalCluster
            {
                Name = ClusterName,
                Register = DateTime.Now,

                Private = true,
                SelfHost = false,

                InstanceID = instance.ID,
                WorkerNode = new List<WorkerNode>(),
                
                OwnerID = UserID,
            };

            _db.Clusters.Add(cluster);
            await _db.SaveChangesAsync();


            return Ok(instance.IPAdress);
        }

        [Manager]
        [HttpPost("Cluster/Unregister")]
        public async Task<IActionResult> UnregisterCluster(string ClusterName)
        {
            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());
            var cluster = _db.Clusters
                .Where(x => x.Name == ClusterName && 
                            x.OwnerID == UserID && 
                           !x.Unregister.HasValue).First();
            if(cluster == null) { BadRequest("cluster not found"); }

            var clusterRequest = new RestRequest(_config.AutoScaling + "/Instance/Terminate")
                .AddQueryParameter("ID",cluster.instance.ID.ToString());
            clusterRequest.Method = Method.POST;

            var client = new RestClient();
            client.Timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
            var clusterResult = await client.ExecuteAsync(clusterRequest); 
            if(clusterResult == null)
            {
                return BadRequest("Fail to terminate cluster");
            }

            var success = JsonConvert.DeserializeObject<bool>(clusterResult.Content);
            if (success)
            {
                cluster.Unregister = DateTime.Now;
                _db.Update(cluster);
                await _db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
