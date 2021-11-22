using MersenneTwister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;
using SharedHost.Models;
using WorkerManager.Interfaces;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Collections.Generic;
using SharedHost.Models.Session;

// TODO: authentification

namespace WorkerManager.Controllers
{
    [Route("/Device")]
    [ApiController]
    [Produces("application/json")]
    public class SessionController: Controller
    {

        private readonly IWorkerNodePool _slavePool;

        public SessionController(SystemConfig config, 
                                  IWorkerNodePool slavePool)
        {
            _slavePool = slavePool;
        }

        
        [HttpPost("SessionMetric")]
        public async Task<IActionResult> Post([FromBody]SessionMetric metric)
        {
            return Ok();
        }

    }
}
