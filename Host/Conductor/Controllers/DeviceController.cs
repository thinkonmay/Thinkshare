using System.Threading.Tasks;
using Conductor.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conductor.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Route("/Device")]
    [ApiController]
    [Produces("application/json")]
    public class DeviceController : Controller
    {


        private readonly ISlaveManagerSocket _slmsocket;

        public DeviceController(ISlaveManagerSocket slmSocket)
        {
            _slmsocket = slmSocket;
        }
        /// <summary>
        /// Add a specific Slave device 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        public async Task<IActionResult> AddSlaveDevice(int ID)
        {
            return await _slmsocket.AddSlaveId(ID) ? Ok() : BadRequest();
        }

        /// <summary>
        /// Reject slave from slavepool but still keep it device infromation in database. 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpDelete("Reject")]
        public async Task<IActionResult> RejectSlave(int ID)
        {
            return await _slmsocket.RejectSlave(ID) ? Ok() : BadRequest();
        }


        /// <summary>
        /// Disconnect slave but still keep its ID in slavepool
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpDelete("Disconnect")]
        public async Task<IActionResult> DisconnectSlave(int ID)
        {
            return await _slmsocket.DisconnectSlave(ID) ? Ok() : BadRequest();
        }
    }
}
