﻿using System.Threading.Tasks;
using Conductor.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;

namespace Conductor.Controllers
{
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
        /// 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="ProcessID"></param>
        /// <returns></returns>
        [HttpPost("Initialize")]
        public IActionResult InitializeCommandlineSession(int SlaveID, int ProcessID)
        {
            _slmsocket.InitializeCommandLineSession(SlaveID, ProcessID);
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
            _slmsocket.TerminateCommandLineSession(SlaveID, ProcessID);
            return Ok();
        }


        /// <summary>
        /// Send a command line to an specific process id of an specific slave device
        /// </summary>
        /// <param name="slave_id"></param>
        /// <param name="process_id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("ForwardCommand")]
        public async Task<IActionResult> CommandLine([FromBody] ForwardCommand command)
        {
            await _slmsocket.SendCommand(command);
            return Ok();
        }
    }
}