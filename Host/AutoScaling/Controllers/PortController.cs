
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.AWS;
using DbSchema.SystemDb.Data;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost;
using RestSharp;
using System.Linq;
using AutoScaling.Interfaces;
using Microsoft.Extensions.Options;

namespace AutoScaling.Controllers
{
    /// <summary>
    /// Routes used by user to fetch information about the system
    /// </summary>
    [Manager]
    [ApiController]
    [Route("/ManagedCluster")]
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


        [Manager]
        [HttpGet("Portforward/Request")]
        public async Task<IActionResult> request(string ClusterName, int LocalPort)
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

                cluster.instance.portForwards.Add(new PortForward{
                    LocalPort = LocalPort,
                    InstancePort = InstancePort,
                    Start = DateTime.Now
                });

                _db.Update(cluster);
                await _db.SaveChangesAsync();
                return Ok();
            }

        }

        [Manager]
        [HttpGet("Portforward/Release")]
        public async Task<IActionResult> Release(string ClusterName, int LocalPort)
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
                var port = cluster.instance.portForwards.Where( x => 
                    x.LocalPort == LocalPort &&
                    !x.End.HasValue
                ).First();
                port.End = DateTime.Now;

                _db.Update(port);
                await _db.SaveChangesAsync();
                return Ok();
            }
        }
    }
}