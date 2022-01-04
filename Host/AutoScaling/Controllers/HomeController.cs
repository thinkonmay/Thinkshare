using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Amazon.EC2;
using Amazon.EC2.Model;


namespace AutoScaling.Controllers
{
    [Route("/Instance")]
    public class InstanceController : Controller
    {
        public InstanceController()
        {
        }

        [HttpPost("/Cluster/Create")]
        public async Task<IActionResult> Cluster()
        {
            return Ok();

        }
    }
}
