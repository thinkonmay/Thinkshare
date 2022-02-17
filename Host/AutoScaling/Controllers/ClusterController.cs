using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Collections.Generic;
using DbSchema.SystemDb.Data;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using SharedHost.Auth;
using SharedHost.Models.Cluster;
using SharedHost;
using RestSharp;
using System.Linq;
using DbSchema.CachedState;
using AutoScaling.Interfaces;
using Microsoft.Extensions.Options;
using SharedHost.Models.AWS;
using SharedHost.Auth;

namespace AutoScaling.Controllers
{
    [ApiController]
    [Route("/Cluster")]
    [Produces("application/json")]
    public class ClusterController : Controller
    {
        private readonly GlobalDbContext _db;

        private readonly IGlobalStateStore _cache;

        private readonly SystemConfig _config;
        
        private readonly InstanceSetting _instanceSetting;

        private readonly IEC2Service _ec2;

        public ClusterController(GlobalDbContext db,
                                 IEC2Service ec2,
                                 IOptions<SystemConfig> config,
                                 IOptions<InstanceSetting> instanceSetting,
                                 IGlobalStateStore cache)
        {
            _cache = cache;
            _db = db;
            _instanceSetting = instanceSetting.Value;
            _config = config.Value;
            _ec2 = ec2;
        }





        [Manager]
        [HttpGet("Token")]
        public async Task<IActionResult> NewCluster(string ClusterName)
        {
            var ManagerID = Int32.Parse(HttpContext.Items["UserID"].ToString());
            var cluster = _db.Clusters.Where(x => x.Name == ClusterName && 
                                                  x.OwnerID == ManagerID).First();

            var request = new RestRequest($"{_config.Authenticator}/Token/Grant/Cluster",Method.POST)
                .AddJsonBody(cluster);
            var result = await (new RestClient()).ExecuteAsync(request);

            var Token = JsonConvert.DeserializeObject<string>(result.Content);
            return Ok(new AuthenticationRequest{
                token = Token,
                Validator = "Autoscaling",
            });
        }

        [Cluster]
        [HttpDelete("Unregister")]
        public async Task<IActionResult> UnregisterCluster()
        {
            var ClusterID = HttpContext.Items["ClusterID"];
            var cluster = _db.Clusters.Find(Int32.Parse(ClusterID.ToString()));

            if(cluster == null)  
                BadRequest("cluster not found"); 

            var success = await _ec2.TerminateInstance(cluster.instance);

            if(!success)
                return BadRequest("fail to terminate instance");

            cluster.instance.End = DateTime.Now;
            cluster.instance.portForwards.ForEach(x => x.End = DateTime.Now);
            cluster.Unregister = DateTime.Now;
            _db.Update(cluster);
            await _db.SaveChangesAsync();
            return Ok();
        }


        [Cluster]
        [HttpPost("Worker/Register")]
        public async Task<IActionResult> Register([FromBody] WorkerRegisterModel body)
        {
            var ClusterID = HttpContext.Items["ClusterID"];
            var Cluster = _db.Clusters.Find(Int32.Parse(ClusterID.ToString()));
            var newWorker = new WorkerNode
            {
                Register = DateTime.Now,
                CPU = body.CPU,
                GPU = body.GPU,
                RAMcapacity = body.RAMcapacity,
                OS = body.OS,
                Name = body.Name,
                User = body.User
            };

            Cluster.WorkerNode.Add(newWorker);
            _db.Clusters.Update(Cluster);
            await _db.SaveChangesAsync();

            await _cache.CacheWorkerInfor(newWorker);
            return Ok(newWorker.ID);
        }


        [Cluster]
        [HttpGet("Infor")]
        public async Task<IActionResult> getInfor()
        {
            var ClusterID = HttpContext.Items["ClusterID"];
            var Cluster = _db.Clusters.Find(Int32.Parse(ClusterID.ToString()));
            Cluster.WorkerNode = null;
            Cluster.Owner = null;
            return Ok(Cluster);
        }

        [Cluster]
        [HttpGet("Instance")]
        public async Task<IActionResult> getInstance()
        {
            var ClusterID = HttpContext.Items["ClusterID"];
            var Cluster = _db.Clusters.Find(Int32.Parse(ClusterID.ToString()));
            return Ok(Cluster.instance);
        }
    }
}