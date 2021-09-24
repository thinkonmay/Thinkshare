using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using SlaveManager.Interfaces;
using RestSharp;
using SlaveManager.SlaveDevices;
using SharedHost.Models.Device;
using System.Collections.Generic;
using SharedHost.Models.Shell;
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
        public IActionResult InitializeShellSession([FromBody] ShellScript script)
        {
            _slavePool.InitShellSession(script);
            return Ok();
        }
    }
}