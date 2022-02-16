using SharedHost.Models.Device;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.AWS;
using DbSchema.SystemDb.Data;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Auth;
using SharedHost;
using RestSharp;
using System.Linq;
using AutoScaling.Interfaces;
using Microsoft.Extensions.Options;

namespace AutoScaling.Controllers
{
    [ApiController]
    [Route("/Port")]
    [Produces("application/json")]
    public class PortController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly GlobalDbContext _db;

        private readonly RestClient _client;

        private readonly SystemConfig _config;
        
        private readonly InstanceSetting _instanceSetting;

        private readonly IEC2Service _ec2;

        public PortController(GlobalDbContext db,
                                 IEC2Service ec2,
                                 IOptions<SystemConfig> config,
                                 IOptions<InstanceSetting> instanceSetting,
                                 UserManager<UserAccount> userManager)
        {
            _db = db;
            _client = new RestClient(config.Value.Authenticator);
            _instanceSetting = instanceSetting.Value;
            _userManager = userManager;
            _config = config.Value;
            _ec2 = ec2;
        }


        [Cluster]
        [HttpGet("Request")]
        public async Task<IActionResult> request()
        {
            var ClusterID = HttpContext.Items["ClusterID"];
            var cluster = _db.Clusters.Find(Int32.Parse(ClusterID.ToString()));

            int InstancePort = 0;
            for (int i = _instanceSetting.PortMinValue; i < _instanceSetting.PortMaxValue; i++)
            {
                if(cluster.instance.portForwards
                    .Where(x => x.InstancePort == i && 
                                !x.End.HasValue)
                    .Count() == 0)
                {
                    InstancePort = i;
                    break;
                }
            }
            var port = new PortForward{
                LocalPort = InstancePort,
                InstancePort = InstancePort,
                Start = DateTime.Now
            };

            cluster.instance.portForwards.Add(port);
            _db.Update(cluster);
            await _db.SaveChangesAsync();

            return Ok(port);
        }

        [Cluster]
        [HttpGet("Release")]
        public async Task<IActionResult> Release(int InstancePort)
        {
            var ClusterID = HttpContext.Items["ClusterID"];
            var cluster = _db.Clusters.Find(Int32.Parse(ClusterID.ToString()));

            var port = cluster.instance.portForwards.Where( x => 
                    x.InstancePort == InstancePort &&
                !x.End.HasValue).First();
            port.End = DateTime.Now;

            _db.Update(port);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}