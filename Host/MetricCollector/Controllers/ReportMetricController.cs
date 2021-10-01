using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MetricCollector.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using MetricCollector.Model;
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

        [HttpGet("RemoteControl")]
        public IActionResult QoSMetric([FromBody] QoSMetric model)
        {
            return Ok();
        }
    }
}
