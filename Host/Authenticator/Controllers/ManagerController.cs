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
using SharedHost.Auth.ThinkmayAuthProtocol;
using DbSchema.SystemDb.Data;
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

        public ManagerController(UserManager<UserAccount> userManager,
                                 ITokenGenerator token,
                                 IOptions<SystemConfig> config,
                                 GlobalDbContext db)
        {
            _userManager = userManager;
            _db = db;
            _token = token;
            _config = config.Value;
        }



        [User]
        [HttpPost("Request")]
        public async Task<IActionResult> Elevate(string Description, 
                                                 bool SelfHost, 
                                                 string ClusterName)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            
            var result = await _userManager.AddToRoleAsync(account,RoleSeeding.MOD);
            Serilog.Log.Information(UserID.ToString() + " want to elevate to manager, Description: "+Description);
            return Ok(result);
        }

        [Manager]
        [HttpPost("ManagedCluster/Request")]
        public async Task<IActionResult> RequestCluster( string ClusterName,
                                                         bool Private)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            if(account.ManagedCluster.Where(x => x.Name == ClusterName).Any())
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

                Private = Private,
                SelfHost = false,

                InstanceID = instance.ID,
                WorkerNode = new List<WorkerNode>()
            };

            account.ManagedCluster.Add(cluster);
            await _userManager.UpdateAsync(account);


            return Ok(instance.IPAdress);
        }

        [Manager]
        [HttpPost("Cluster/Unregister")]
        public async Task<IActionResult> UnregisterCluster(string ClusterName)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());

            var cluster = account.ManagedCluster.Where(x => x.Name == ClusterName).First();



            var clusterRequest = new RestRequest(_config.AutoScaling + "/Instance/Terminate")
                .AddJsonBody(cluster.instance);
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
                await _userManager.UpdateAsync(account);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
