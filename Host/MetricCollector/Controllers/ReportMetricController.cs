using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MetricCollector.Interface;
using SharedHost;
using Microsoft.Extensions.Options;
using SharedHost.Auth;

namespace MetricCollector.Controllers
{
    [ApiController]
    [Route("/Metric")]
    [Produces("application/json")]
    public class ReportMetricController : ControllerBase
    {

        private readonly ILogger<MetricController> _logger;

        private readonly IScriptGetter _getter;
        private readonly SystemConfig _config;

        public ReportMetricController(ILogger<MetricController> logger, IScriptGetter getter, IOptions<SystemConfig> config)
        {
            _getter = getter;
            _logger = logger;
            _config = config.Value;
        }

        [Cluster]
        [HttpPost("Cluster")]
        public IActionResult ClusterLog ([FromBody] string content)
        {
            var ClusterID = HttpContext.Items["ClusterID"];
            Serilog.Log.Information($"[Cluster {ClusterID} log] {content}");
            return Ok();
        }
    }
}
