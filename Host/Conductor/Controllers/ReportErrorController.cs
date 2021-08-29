using Conductor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conductor.Controllers
{
    [Route("/Error")]
    [ApiController]
    public class ReportErrorController : Controller
    {
        private readonly IAdmin _admin;

        public ReportErrorController(IAdmin admin)
        {
            _admin = admin;
        }

        [HttpPost("Report")]
        public async Task<IActionResult> ReportError(ReportedError error)
        {
            await _admin.ReportError(error);
            return Ok();
        }
    }
}
