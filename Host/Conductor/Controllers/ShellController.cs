using System.Threading.Tasks;
using Conductor.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;
using SharedHost.Models.Command;

namespace Conductor.Controllers
{
    /// <summary>
    /// Route use by admin to create shell remote session with slave devices
    /// </summary>
    [Authorize(Roles = "Administrator")]
    [Route("/Shell")]
    [ApiController]
    public class ShellController : Controller
    {
        private readonly ISlaveManagerSocket _slmsocket;

        public ShellController(ISlaveManagerSocket slmSocket)
        {
            _slmsocket = slmSocket;
        }




        /// <summary>
        /// Send a command line to an specific process id of an specific slave device
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("ShellScript")]
        public async Task<IActionResult> Shell([FromBody] ShellScript command)
        {
            if((await _slmsocket.GetSlaveState(command.SlaveID)).SlaveServiceState == SlaveServiceState.Disconnected)
            {
                return BadRequest("Device not available");
            }
            await _slmsocket.InitializeShellSession(command);
            return Ok();
        }
    }
}
