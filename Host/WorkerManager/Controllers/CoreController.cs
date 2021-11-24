
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
    [Route("Session")]
    [Produces("application/json")]
    public class CoreController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ClusterDbContext _db;

        public CoreController(IWorkerNodePool slavePool, 
                            ClusterDbContext db, 
                            ITokenGenerator token)
        {
            _db = db;
            _tokenGenerator = token;
        }


        [HttpGet("Token")]
        public async Task<IActionResult> GetSessionToken(string token)
        {
            var result = await _tokenGenerator.ValidateToken(token);
            return Ok(result.RemoteToken);
        }

        [HttpGet("QoE")]
        public async Task<IActionResult> GetSessionQoE(string token)
        {
            var result = await _tokenGenerator.ValidateToken(token);
            return Ok(result.QoE);
        }

        [HttpGet("Signalling")]
        public async Task<IActionResult> GetSignalling(string token)
        {
            var result = await _tokenGenerator.ValidateToken(token);
            return Ok(result.SignallingUrl);
        }
    }
}
