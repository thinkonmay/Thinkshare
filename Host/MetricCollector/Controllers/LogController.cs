using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Shell;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MetricCollector.Interface;
using SharedHost;
using Microsoft.Extensions.Options;
using SharedHost.Models.Logging;
using SharedHost.Logging;
namespace MetricCollector.Controllers
{
    [ApiController]
    [Route("/Log")]
    [Produces("application/json")]
    public class LogController : ControllerBase
    {
        private readonly IScriptGetter _getter;

        private readonly SystemConfig _config;

        private readonly ILog _log;

        public LogController(ILog log, 
                            IScriptGetter getter, 
                            IOptions<SystemConfig> config)
        {
            _log = log;
            _getter = getter;
            _config = config.Value;
        }

        [HttpPost("Infor")]
        public async Task<IActionResult> Infor([FromBody] GenericLogModel session)
        {
            _log.Cluster(session);
            return Ok();
        }
        [HttpPost("Error")]
        public async Task<IActionResult> Error([FromBody] ErrorLogModel session)
        {
            _log.Cluster(session,"error");
            return Ok();
        }
        [HttpPost("Fatal")]
        public async Task<IActionResult> Fatal([FromBody] ErrorLogModel session)
        {
            _log.Cluster(session,"fatal");
            return Ok();
        }
        [HttpPost("Worker")]
        public async Task<IActionResult> Worker([FromBody] GenericLogModel session)
        {
            _log.Worker(session);
            return Ok();
        }
    }
}
