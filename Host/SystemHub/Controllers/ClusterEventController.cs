﻿using Microsoft.AspNetCore.Mvc;
using SystemHub.Interfaces;
using SharedHost.Models.Device;

namespace SystemHub.Controllers
{
    [Route("Cluster/Worker")]
    [ApiController]
    [Produces("application/json")]
    public class ClusterEventController : ControllerBase
    {
        private readonly IClusterSocketPool _Cluster;

        public ClusterEventController(IClusterSocketPool queue)
        {
            _Cluster = queue;
        }

        [HttpPost("Initialize")]
        public IActionResult ClientPost(int WorkerID, string token)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_INITIALIZE,
                WorkerID = WorkerID,
                token = token,
            };
            _Cluster.SendToNode(message);
            return Ok();
        }

        [HttpPost("Terminate")]
        public IActionResult Terminate(int WorkerID)

        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_TERMINATE,
                WorkerID = WorkerID,
            };
            _Cluster.SendToNode(message);
            return Ok();
        }

        [HttpPost("Disconnect")]
        public IActionResult Disconnect(int WorkerID)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_DISCONNECT,
                WorkerID = WorkerID,
            };
            _Cluster.SendToNode(message);
            return Ok();
        }

        [HttpPost("Reconnect")]
        public IActionResult Reconnect(int WorkerID)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_RECONNECT,
                WorkerID = WorkerID,
            };
            _Cluster.SendToNode(message);
            return Ok();
        }
    }
}
