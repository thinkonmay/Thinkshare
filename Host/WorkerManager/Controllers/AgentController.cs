<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Mvc;
=======
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
>>>>>>> 30990201458f89c35a5afe73f30da27c8f50055d
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using WorkerManager.SlaveDevices;
using SharedHost.Models.Device;
using WorkerManager.Data;
<<<<<<< HEAD
=======
using WorkerManager.Middleware;
using System.Linq;
>>>>>>> 30990201458f89c35a5afe73f30da27c8f50055d

namespace WorkerManager.Controllers
{
    [ApiController]
<<<<<<< HEAD
    [Route("/Agent")]
    [Produces("application/json")]
    public class AgentController : ControllerBase
=======
    [Route("/agent")]
    [Produces("application/json")]
    public class WebSocketApiController : ControllerBase
>>>>>>> 30990201458f89c35a5afe73f30da27c8f50055d
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ClusterDbContext _db;

<<<<<<< HEAD
        public AgentController(IWorkerNodePool slavePool, ClusterDbContext db, ITokenGenerator token)
=======
        public WebSocketApiController(IWorkerNodePool slavePool, ClusterDbContext db, ITokenGenerator token)
>>>>>>> 30990201458f89c35a5afe73f30da27c8f50055d
        {
            _db = db;
            _tokenGenerator = token;
        }

<<<<<<< HEAD
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
                agentUrl = "http://"+agentip+":"+agentport.ToString()+"/agent",
                coreUrl = "http://"+agentip+":"+coreport.ToString()+"/core",
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
=======
        /// <summary>
        /// 
        /// </summary>
        /// <param name="managerToken"></param>
        /// <param name="token"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("Register")]
        public async Task<IActionResult> PostInfor([FromBody]ClusterWorkerNode node)
        {
            var current = DateTime.Now;
            node._workerState = WorkerState.Unregister;
            node.Register = current; 

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
>>>>>>> 30990201458f89c35a5afe73f30da27c8f50055d
            return Ok();
        }
    }
}
