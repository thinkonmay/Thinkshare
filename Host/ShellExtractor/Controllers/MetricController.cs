using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MetricCollector.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetricCollector.Controllers
{
    [ApiController]
    [Route("/Metric")]
    [Produces("application/json")]
    public class MetricController : ControllerBase
    {

        private readonly ILogger<MetricController> _logger;

        private readonly IScriptGetter _getter;

        public MetricController(ILogger<MetricController> logger, IScriptGetter getter)
        {
            _getter = getter;
            _logger = logger;
        }

        [HttpGet("CPU")]
        public IActionResult CPUMetric(int SlaveID)
        {
            return Ok(_getter.GetCPU(SlaveID));
        }

        [HttpGet("GPU")]
        public IActionResult GPUMetric(int SlaveID)
        {
            return Ok(_getter.GetGPU(SlaveID));
        }

        [HttpGet("RAM")]
        public IActionResult RAMMetric(int SlaveID)
        {
            return Ok(_getter.GetRAM(SlaveID));
        }

        [HttpGet("Storage")]
        public IActionResult StorageMetric(int SlaveID)
        {
            return Ok(_getter.GetStorage(SlaveID));
        }

    }
}
