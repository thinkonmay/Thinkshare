using System;
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

        [HttpPost("/Cluster/Create")]
        public async Task<IActionResult> Cluster()
        {
            var result = await _ec2.SetupCoturnService();
            return Ok(result);
        }
    }
}
