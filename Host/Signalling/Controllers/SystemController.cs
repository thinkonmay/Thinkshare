using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedHost.Models.Session;
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
    [Produces("application/json")]
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
        public IActionResult AddSessionPair([FromBody] SessionPair session)
        {
            var ret = _queue.AddSessionPair(session);
            if (ret)
            {
                return Ok("Added session pair");
            }else
            {
                return BadRequest("Session pair already contain in queue");
            }
        }

        [HttpPost("Terminate")]
        public IActionResult TerminateSessionPair([FromBody] SessionPair session)
        {
            var ret = _queue.RemoveIDPair(session);
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
        public IActionResult GetCurrentSession()
        {            
            return Ok(_queue.GetSessionPair());
        }

        [HttpGet("GetOnlineDevice")]
        public IActionResult GetOnlineDevice()
        {
            return Ok(_queue.GetOnlineList());
        }
    }
}
