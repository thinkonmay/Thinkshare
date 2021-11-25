using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using WorkerManager.Data;
using WorkerManager.Middleware;
using WorkerManager.Models;
using System.Linq;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/Agent")]
    [Produces("application/json")]
    public class AgentController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ClusterDbContext _db;

        public AgentController(IWorkerNodePool slavePool, ClusterDbContext db, ITokenGenerator token)
        {
            _db = db;
            _tokenGenerator = token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="managerToken"></param>
        /// <param name="token"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("Register")]
        public async Task<IActionResult> PostInfor([FromBody]WorkerRegisterModel agent_register)
        {
            var current = DateTime.Now;
            var node = new ClusterWorkerNode
            {
                Register = current,
                _workerState = WorkerState.Unregister,
                PrivateIP = agent_register.LocalIP,
                CPU = agent_register.CPU,
                GPU = agent_register.GPU,
                RAMcapacity = (int)agent_register.RAMcapacity,
                OS = agent_register.OS,
            }; 

            _db.Devices.Add(node);
            var device = _db.Devices.Where(o => o.Register == current && o._workerState == WorkerState.Unregister).First();
            return Ok(await _tokenGenerator.GenerateWorkerToken(device));
        }

        [Worker]
        [HttpPost("SessionCoreEnd")]
        public async Task<IActionResult> Post()
        {
            var workerID = HttpContext.Items["PrivateID"];
            var device = _db.Devices.Find(workerID);
            if(device._workerState == WorkerState.OnSession)
            {
                device._workerState = WorkerState.OffRemote;
            }
            return Ok();
        }
    }
}
