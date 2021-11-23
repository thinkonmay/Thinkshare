using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost;
using WorkerManager.Middleware;

// TODO: authentification

namespace WorkerManager.Controllers
{
    [Route("/collect")]
    [ApiController]
    [Produces("application/json")]
    public class SessionController: Controller
    {

        private readonly IWorkerNodePool _slavePool;

        public SessionController(SystemConfig config, 
                                  IWorkerNodePool slavePool)
        {
            _slavePool = slavePool;
        }

        [Worker]
        [HttpPost("session")]
        public async Task<IActionResult> Session([FromBody]SessionMetric metric)
        {
            return Ok();
        }

        [Worker]
        [HttpPost("log")]
        public async Task<IActionResult> Log([FromBody] SessionMetric metric)
        {
            return Ok();
        }
    }
}
