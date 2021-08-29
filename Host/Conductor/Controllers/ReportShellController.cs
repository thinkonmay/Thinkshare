using Conductor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conductor.Controllers
{
    [Route("/ReportShell")]
    [ApiController]
    public class ReportShellController : Controller
    {
        private readonly IAdmin _admin;

        public ReportShellController(IAdmin admin)
        {
            _admin = admin;
        }
        [HttpPost("Terminated")]
        public async Task<IActionResult> ReportShellSessionTerminated(int SlaveID, int ProcessID)
        {
            await _admin.ReportShellSessionTerminated(SlaveID, ProcessID);
            return Ok();
        }


        [HttpPost("Output")]
        public async Task<IActionResult> LogCommandline(ReceiveCommand command)
        {
            await _admin.LogSlaveCommandLine(command);
            return Ok();
        }
    }
}
