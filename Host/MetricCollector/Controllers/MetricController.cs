using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MetricCollector.Interface;
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
        public async Task<IActionResult> CPUMetric(int SlaveID)
        {
            return Ok(await _getter.GetCPU(SlaveID));
        }

        [HttpGet("GPU")]
        public async Task<IActionResult> GPUMetric(int SlaveID)
        {
            return Ok(await _getter.GetGPU(SlaveID));
        }

        [HttpGet("RAM")]
        public async Task<IActionResult> RAMMetric(int SlaveID)
        {
            return Ok(await _getter.GetRAM(SlaveID));
        }

        [HttpGet("Storage")]
        public async Task<IActionResult> StorageMetric(int SlaveID)
        {
            return Ok(await _getter.GetStorage(SlaveID));
        }

    }
}
