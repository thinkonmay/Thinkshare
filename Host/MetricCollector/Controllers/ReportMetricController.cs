using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Shell;
using System.Collections.Generic;
using SharedHost.Auth.ThinkmayAuthProtocol;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MetricCollector.Interface;
using SharedHost;
using Microsoft.Extensions.Options;

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
        [HttpPost("Add")]
        public async Task<IActionResult> UpdateShellSession([FromBody] List<ShellSession> session)
        {
            var ClusterID = HttpContext.Items["ClusterID"];
            return Ok();
        }
    }
}
