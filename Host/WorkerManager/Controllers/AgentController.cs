using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using WorkerManager.Data;
using WorkerManager.Middleware;
using System.Linq;
using DbSchema.CachedState;
using SharedHost.Models.Local;
using MersenneTwister;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/Agent")]
    [Produces("application/json")]
    public class AgentController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ClusterDbContext _db;

        private readonly LocalStateStore _cache;

        public AgentController( ClusterDbContext db, 
                                ITokenGenerator token,
                                LocalStateStore cache)
        {
            _cache = cache;
            _db = db;
            _tokenGenerator = token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent_register"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("Register")]
        public async Task<IActionResult> PostInfor([FromBody]WorkerRegisterModel agent_register)
        {
            var current = DateTime.Now;
            var node = new ClusterWorkerNode
            {
                Register = current,
                PrivateIP = agent_register.LocalIP,
                CPU = agent_register.CPU,
                GPU = agent_register.GPU,
                RAMcapacity = (int)agent_register.RAMcapacity,
                OS = agent_register.OS,
            };

            _db.Devices.Add(node);
            await _db.SaveChangesAsync();

            await _cache.SetWorkerState(node.PrivateID, WorkerState.Unregister);
            return Ok(await _tokenGenerator.GenerateWorkerToken(node));
        }

        [Worker]
        [HttpPost("EndRemote")]
        public async Task<IActionResult> Post()
        {
            var workerID = Int32.Parse((string)HttpContext.Items["PrivateID"]);
            var state = await _cache.GetWorkerState(workerID);
            if(state == WorkerState.OnSession)
            {
                await _cache.SetWorkerState(workerID, WorkerState.OffRemote);
            }
            return Ok();
        }
    }
}
