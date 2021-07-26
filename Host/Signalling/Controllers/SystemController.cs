using Microsoft.AspNetCore.Mvc;
using Signalling.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signalling.Controllers
{
    [Route("/System")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly IWebSocketHandler _wsHandler;
        private readonly ISessionQueue _queue;

        public SystemController(IWebSocketHandler wsHandler, ISessionQueue queue)
        {
            _wsHandler = wsHandler;
            _queue = queue;
        }

        [HttpPost("Generate")]
        public IActionResult AddSessionPair(int slaveID, int clientID)
        {
            _queue.AddSessionPair(slaveID, clientID);
            return Ok("Added session pair");
        }

        [HttpPost("Terminate")]
        public IActionResult TerminateSessionPair(int slaveID, int clientID)
        {
            _queue.RemoveIDPair(slaveID, clientID);
            return Ok("Terminated session pair");
        }

        [HttpGet("GetCurrentSession")]
        public List<Tuple<int,int>> GetCurrentSession()
        {
            
            return _queue.GetSessionPair();
        }

        [HttpGet("GetOnlineDevice")]
        public List<int> GetOnlineDevice()
        {

            return _queue.GetOnlineList();
        }
    }
}
