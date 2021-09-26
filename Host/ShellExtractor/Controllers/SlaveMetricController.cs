using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShellExtractor.Controllers
{
    [ApiController]
    [Route("/SlaveMetric")]
    [Produces("application/json")]
    public class SlaveMetricController : ControllerBase
    {

        private readonly ILogger<SlaveMetricController> _logger;

        public SlaveMetricController(ILogger<SlaveMetricController> logger)
        {
            _logger = logger;
        }

        [HttpGet("CPU")]
        public async Task<IActionResult> CPUMetric()
        {

            return Ok();
        }

        [HttpGet("GPU")]
        public async Task<IActionResult> GPUMetric()
        {

            return Ok();
        }

        [HttpGet("RAM")]
        public async Task<IActionResult> RAMMetric()
        {

            return Ok();
        }

        [HttpGet("Storage")]
        public async Task<IActionResult> StorageMetric()
        {

            return Ok();
        }

    }
}
