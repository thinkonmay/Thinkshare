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

        [HttpPost("Register")]
        public async Task<IActionResult> Register(string agentip, 
                                                    int agentport, 
                                                    int coreport, 
                                                    string GPU, 
                                                    string CPU, 
                                                    int RAMCapacity, 
                                                    string OS)
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
                RAMcapacity = RAMCapacity,
                OS = OS
            };
            _db.Devices.Add(worker);
            return Ok(await _tokenGenerator.GenerateJwt(worker));
        }


        [HttpGet("SessionToken")]
        public async Task<IActionResult> GetSessionToken(string token)
        {
            var result = await _tokenGenerator.ValidateToken(token);
            return Ok(result.RemoteToken);
        }

        [HttpGet("GetState")]
        public async Task<IActionResult> GetState(string token)
        {
            var result = await _tokenGenerator.ValidateToken(token);
            return Ok(result._workerState);
        }


        [HttpPost("SetState")]
        public async Task<IActionResult> SetState(string token, string WorkerState)
        {
            var result = await _tokenGenerator.ValidateToken(token);
            var device = _db.Devices.Find(result.PrivateID);
            device._workerState = WorkerState;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
