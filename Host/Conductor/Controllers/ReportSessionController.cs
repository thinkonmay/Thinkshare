using Conductor.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("Disconnected")]
        public async Task<IActionResult> SessionDisconnected(int SlaveID)
        {
            await _admin.ReportRemoteControlDisconnected(SlaveID);
            return Ok();
        }
    }
}
