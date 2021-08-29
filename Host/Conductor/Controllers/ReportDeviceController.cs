using Conductor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conductor.Controllers
{
    [Route("/ReportDevices")]
    [ApiController]
    public class ReportDeviceController : Controller
    {
        private readonly IAdmin _admin;

        public ReportDeviceController(IAdmin admin)
        {
            _admin = admin;
        }

        [HttpPost("Registered")]
        public async Task<IActionResult> ReportSlaveRegistered(SlaveDeviceInformation infor)
        {
            await _admin.ReportSlaveRegistered(infor);
            return Ok();
        }

        [HttpPost("Disconnected")]
        public async Task<IActionResult> ReportSlaveDisconnected(int SlaveID)
        {
            await _admin.ReportSlaveDisconnected(SlaveID);
            return Ok();
        }
    }
}
