using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using WorkerManager.SlaveDevices;
using SharedHost.Models.Device;
using WorkerManager.Data;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/Agent")]
    [Produces("application/json")]
    public class WebSocketApiController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ClusterDbContext _db;

        public WebSocketApiController(IWorkerNodePool slavePool, ClusterDbContext db, ITokenGenerator token)
        {
            _db = db;
            _tokenGenerator = token;
        }

        [HttpGet("Register")]
        public async Task<IActionResult> Get(string agentip, int agentport, int coreport, string GPU, string CPU, int RAMCapacity, string OS)
        {
            var worker = new ClusterWorkerNode
            {
                _workerState = WorkerState.Unregister,
                Register = DateTime.Now,
                agentUrl = "http://"+agentip+":"+agentport.ToString(),
                coreUrl = "http://"+agentip+":"+coreport.ToString(),
                PrivateIP = agentip,
                GPU = GPU,
                CPU = CPU,
                RAMCapacity = RAMCapacity,
                OS = OS
            };
            _db.Devices.Add(worker);
            return Ok(await _tokenGenerator.GenerateJwt(worker));
        }
    }
}
