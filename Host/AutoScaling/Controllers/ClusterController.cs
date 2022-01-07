using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.AWS;
using DbSchema.SystemDb.Data;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost.Models.Cluster;
using SharedHost;
using RestSharp;
using System.Linq;
using DbSchema.CachedState;
using AutoScaling.Interfaces;
using Microsoft.Extensions.Options;

namespace AutoScaling.Controllers
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

        private readonly IGlobalStateStore _cache;

        private readonly RestClient _client;

        private readonly SystemConfig _config;
        
        private readonly InstanceSetting _instanceSetting;

        private readonly IEC2Service _ec2;

        public ClusterController(GlobalDbContext db,
                                 IEC2Service ec2,
                                 IOptions<SystemConfig> config,
                                 IOptions<InstanceSetting> instanceSetting,
                                 UserManager<UserAccount> userManager,
                                 IGlobalStateStore cache)
        {
            _cache = cache;
            _db = db;
            _client = new RestClient(config.Value.Authenticator);
            _instanceSetting = instanceSetting.Value;
            _userManager = userManager;
            _config = config.Value;
            _ec2 = ec2;
        }





        [Manager]
        [HttpPost("Register")]
        public async Task<IActionResult> NewCluster(string ClusterName, bool Private)
        {
            GlobalCluster cluster;
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account =  await _userManager.FindByIdAsync((string)ManagerID);

            var refreshCluster = account.ManagedCluster.Where(x => x.Name == ClusterName);
            if(refreshCluster.Count() == 0)
            {
                cluster = new GlobalCluster
                {
                    Name = ClusterName,
                    Register = DateTime.Now,
                    Private = Private
                };
                account.ManagedCluster.Add(cluster);
                await _userManager.UpdateAsync(account);
            }
            else
            {
                cluster = refreshCluster.First();
                if(cluster.SelfHost)
                {
                    if(cluster.instance == null)
                    {
                        cluster.instance = await _ec2.SetupCoturnService();
                        await _userManager.UpdateAsync(account);
                    }                    
                }
            }


            var request = new RestRequest(_config.ClusterTokenGrantor)
                .AddQueryParameter("UserID",account.Id.ToString())
                .AddQueryParameter("ClusterName",ClusterName)
                .AddQueryParameter("ClusterID",cluster.ID.ToString());
            request.Method = Method.POST;
            
            var result = await _client.ExecuteAsync(request);
            var Token = JsonConvert.DeserializeObject<string>(result.Content);
            return Ok(Token);
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

            await _cache.CacheWorkerInfor(newWorker);
            return Ok(newWorker.ID);
        }

        [Manager]
        [HttpGet("Infor")]
        public async Task<IActionResult> getInfor(string ClusterName)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account = await _userManager.FindByIdAsync((string)ManagerID);
            var cluster = account.ManagedCluster.Where(x => x.Name == ClusterName).First();
            return Ok(cluster);
        }

        [Manager]
        [HttpGet("ManagedInstance")]
        public async Task<IActionResult> getKey(string ClusterName)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account = await _userManager.FindByIdAsync((string)ManagerID);
            var cluster = account.ManagedCluster.Where(x => x.Name == ClusterName).First();
            if(cluster.SelfHost)
            {
                return BadRequest();
            }
            else
            {
                return Ok(cluster.instance);
            }
        }


    }
}