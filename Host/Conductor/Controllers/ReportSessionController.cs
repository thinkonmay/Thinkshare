using Conductor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conductor.Controllers
{
    [Route("/ReportSession")]
    [ApiController]
    public class ReportSessionController : Controller
    {
        private readonly IAdmin _admin;

        public ReportSessionController(IAdmin admin)
        {
            _admin = admin;
        }

        /// <summary>
        /// Slave Manager report session disconected,
        /// this may due to error in session core
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpPost("Disconnected")]
        public async Task<IActionResult> SessionDisconnected(int SlaveID)
        {
            await _admin.ReportRemoteControlDisconnected(SlaveID);
            return Ok();
        }

        /// <summary>
        /// Signalling report session disconnected,
        /// this is not an error and may due to user force exit the remote control
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [HttpPost("Disconnected")]
        public async Task<IActionResult> SessionDisconnectedFromSignalling([FromBody] SessionPair session)
        {
            await _admin.ReportRemoteControlDisconnectedFromSignalling(session);
            return Ok();
        }
    }
}
