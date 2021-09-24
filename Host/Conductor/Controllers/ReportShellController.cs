using Conductor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedHost.Models.Command;

namespace Conductor.Controllers
{
    /// <summary>
    /// Reserve for RESTful request
    /// </summary>
    [Route("/ReportShell")]
    [ApiController]
    public class ReportShellController : Controller
    {
        private readonly IAdmin _admin;

        public ReportShellController(IAdmin admin)
        {
            _admin = admin;
        }

        [HttpPost("Output")]
        public async Task<IActionResult> LogCommandline([FromBody] ShellOutput command)
        {
            await _admin.LogShellOutput(command);
            return Ok();
        }
    }
}
