using Microsoft.AspNetCore.Mvc;
using SlaveManager.Services;
using SlaveManager.Interfaces;






// TODO: disable in publish mode

namespace SlaveManager.Controllers
{

    /*Development Usage only*/
    [Route("/Test")]
    [ApiController]
    public class TestController : Controller
    {
        private readonly ISlavePool _SlavePool;


        public TestController(ISlavePool slavePool)
        {
            _SlavePool = slavePool;
        }

        [HttpGet("GetSlaveState")]
        public IActionResult GetSlaveInPool()
        {
            return Ok(_SlavePool.GetSystemSlaveState());
        }

        [HttpGet("Initialize")]
        public IActionResult SessionInitialize(int id)
        {
            if (_SlavePool.SessionInitialize(id, null))
            {

                return Ok("Test Ok");
            }
            else
            {
                return BadRequest("Slave not found");
            }
        }

        [HttpPost("Termination")]
        public IActionResult SessionTerminate(int id)
        {
            if (_SlavePool.SessionTerminate(id))
            {

                return Ok("Test Ok");
            }
            else
            {
                return BadRequest("Slave not found");
            }
        }

        [HttpPost("Reject")]
        public IActionResult RejectSlave(int id)
        {
            if (_SlavePool.RejectSlave(id))
            {

                return Ok("Test Ok");
            }
            else
            {
                return BadRequest("Slave not found");
            }
        }

        [HttpPost("SSH")]
        public IActionResult CommandLine(int id, int order, string command)
        {
            if (_SlavePool.SendCommand(id, order, command))
            {

                return Ok("Test Ok");
            }
            else
            {
                return BadRequest("Error foward command");
            }
        }
    }
}
