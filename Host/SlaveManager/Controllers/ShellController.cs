using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using SlaveManager.Interfaces;
using RestSharp;
using SlaveManager.SlaveDevices;
using SharedHost.Models.Device;
using System.Collections.Generic;
using SharedHost;

namespace SlaveManager.Controllers
{
    [Route("/Shell")]
    [ApiController]
    [Produces("application/json")]
    public class ShellController : Controller
    {
        private readonly ISlavePool _slavePool;

        private readonly IConductorSocket _conductor;

        private readonly SystemConfig _config;

        public ShellController(ISlavePool slavePool,
                                SystemConfig config,
                                IConductorSocket conductor)
        {
            _config = config;
            _conductor = conductor;
            _slavePool = slavePool;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="ProcessID"></param>
        /// <returns></returns>
        [HttpPost("Initialize")]
        public IActionResult InitializeCommandlineSession(int SlaveID, int ProcessID)
        {
            _slavePool.InitializeCommand(SlaveID, ProcessID);
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="ProcessID"></param>
        /// <returns></returns>
        [HttpPost("Terminate")]
        public IActionResult TerminateCommandlineSession(int SlaveID, int ProcessID)
        {
            _slavePool.TerminateCommand(SlaveID, ProcessID);
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("ForwardCommand")]
        public IActionResult CommandLine(ForwardCommand command)
        {
            return _slavePool.SendCommand(command) ? Ok() : BadRequest();
        }
    }
}