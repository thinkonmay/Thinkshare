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
    [Route("/agent")]
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

        [HttpPost("Register")]
        public async Task<IActionResult> PostInfor([FromBody]ClusterWorkerNode node)
        {
            node._workerState = WorkerState.Unregister;
            node.Register = DateTime.Now;

            _db.Devices.Add(worker);
            return Ok(await _tokenGenerator.GenerateJwt(worker));
        }

        [HttpPost("SessionCoreEnd")]
        public async Task<IActionResult> Post(string token)
        {
            ClusterWorkerNode worker = _tokenGenerator.ValidateToken(token);
            var device = _db.Devices.Find(worker.PrivateID);
            if(device._workerState == WorkerState.OnSession)
            {
                device._workerState = WorkerState.OffRemote;
            }
            return Ok();
        }
    }
}
