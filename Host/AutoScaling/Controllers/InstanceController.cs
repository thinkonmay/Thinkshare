using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Amazon.EC2.Model;
using AutoScaling.Interfaces;
using SharedHost.Models.AWS;

namespace AutoScaling.Controllers
{
    [Route("/Instance")]
    public class InstanceController : Controller
    {
        private readonly IEC2Service  _ec2;

        public InstanceController(IEC2Service ec2)
        {
            _ec2 = ec2;
        }
        [HttpPost("Managed")]
        public async Task<IActionResult> ManagedInstance()
        {
            return Ok(await _ec2.SetupManagedCluster());
        }

        [HttpPost("Terminate")]
        public async Task<IActionResult> Cluster([FromBody]ClusterInstance instance)
        {
            return Ok(await _ec2.TerminateInstance(instance));
        }
    }
}
