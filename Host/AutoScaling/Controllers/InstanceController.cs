using System;
using SharedHost.Models.AWS;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Amazon.EC2;
using Amazon.EC2.Model;
using AutoScaling.Interfaces;

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

        [HttpGet("/Create")]
        public async Task<IActionResult> Cluster()
        {
            return Ok(await _ec2.LaunchInstances());
        }

        [HttpGet("/Terminate")]
        public async Task<IActionResult> Cluster(string ID)
        {
            return Ok(await _ec2.EC2TerminateInstances(ID));
        }

        [HttpPost("/SSH")]
        public async Task<IActionResult> SSH( string IP,[FromBody] List<string> commands)
        {
            return Ok(
                await _ec2.AccessEC2Instance(IP ,commands)
            );
        }

        [HttpGet("/Cluster")]
        public async Task<IActionResult> Coturn()
        {
            return Ok(
                await _ec2.SetupClusterManager()
            );
        }
    }
}
