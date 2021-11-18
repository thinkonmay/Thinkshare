using Conductor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SharedHost.Models.Shell;
using SharedHost.Auth.ThinkmayAuthProtocol;

namespace Conductor.Controllers
{
    /// <summary>
    /// Reserve for RESTful request
    /// </summary>
    [Route("/Shell")]
    [ApiController]
    public class ReportShellController : Controller
    {
        private readonly IAdmin _admin;

        public ReportShellController(IAdmin admin)
        {
            _admin = admin;
        }


        [Manager]
        [HttpPost("Output")]
        public async Task<IActionResult> Shell([FromBody] ShellSession command)
        {
            await _admin.LogShellOutput(command);
            return Ok();
        }
    }
}
