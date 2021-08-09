using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Signalling.Interfaces;
using Signalling.Models;
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
        public IActionResult AddSessionPair(string sspair)
        {
            var sessionPair = JsonConvert.DeserializeObject<SessionPair>(sspair);
            _queue.AddSessionPair(sessionPair.SessionSlaveID, sessionPair.SessionClientID);
            return Ok("Added session pair");
        }

        [HttpDelete("Terminate")]
        public IActionResult TerminateSessionPair(string sspair)
        {
            var sessionPair = JsonConvert.DeserializeObject<SessionPair>(sspair);
            _queue.RemoveIDPair(sessionPair.SessionSlaveID, sessionPair.SessionClientID);
            return Ok("Terminated session pair");
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
