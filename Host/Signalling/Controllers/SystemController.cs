using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Signalling.Filters;
using Signalling.Interfaces;
using Signalling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signalling.Controllers
{
    
    // [ServiceFilter(typeof(ClientIpFilter))]
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
        public IActionResult AddSessionPair(int SessionSlaveID, int SessionClientID)
        {
            var ret = _queue.AddSessionPair(SessionSlaveID, SessionClientID);
            if (ret)
            {
                return Ok("Added session pair");
            }else
            {
                return BadRequest("Session pair already contain in queue");
            }
        }

        [HttpPost("Terminate")]
        public IActionResult TerminateSessionPair(int SessionSlaveID, int SessionClientID)
        {
            var ret = _queue.RemoveIDPair(SessionSlaveID, SessionClientID);
            if (ret)
            {
                return Ok("Terminated session pair");
            }
            else
            {
                return BadRequest("Session pair not exist");
            }
        }

        /// <summary>
        ///
        /// </summary>
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
