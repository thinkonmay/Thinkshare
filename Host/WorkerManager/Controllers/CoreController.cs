using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost;
using WorkerManager.Middleware;
using WorkerManager.Data;
using SharedHost.Models.Device;

// TODO: authentification

namespace WorkerManager.Controllers
{
    [Route("/session")]
    [ApiController]
    [Produces("application/json")]
    public class CoreController : Controller
    {
        private readonly ClusterDbContext _db;

        public CoreController(SystemConfig config,
                              ClusterDbContext db)
        {
            _db = db;
        }

        [Worker]
        [HttpPost("token")]
        public async Task<IActionResult> Session()
        {
            var PrivateID = HttpContext.Items["PrivateID"];
            var Node = _db.Devices.Find(PrivateID);
            if (Node._workerState == WorkerState.OnSession ||
               Node._workerState == WorkerState.OffRemote)
            {
                return Ok(Node.RemoteToken);
            }
            else
            {
                return BadRequest();
            }
        }

        [Worker]
        [HttpPost("signalling")]
        public async Task<IActionResult> Signalling()
        {
            var PrivateID = HttpContext.Items["PrivateID"];
            var Node = _db.Devices.Find(PrivateID);
            if (Node._workerState == WorkerState.OnSession ||
               Node._workerState == WorkerState.OffRemote)
            {
                return Ok(Node.RemoteToken);
            }
            else
            {
                return BadRequest();
            }
        }

        [Worker]
        [HttpPost("turn")]
        public async Task<IActionResult> Turn()
        {
            var PrivateID = HttpContext.Items["PrivateID"];
            var Node = _db.Devices.Find(PrivateID);
            if (Node._workerState == WorkerState.OnSession ||
               Node._workerState == WorkerState.OffRemote)
            {
                return Ok(Node);
            }
            else
            {
                return BadRequest();
            }
        }

        [Worker]
        [HttpPost("qoe")]
        public async Task<IActionResult> QoE()
        {
            var PrivateID = HttpContext.Items["PrivateID"];
            var Node = _db.Devices.Find(PrivateID);
            if (Node._workerState == WorkerState.OnSession ||
               Node._workerState == WorkerState.OffRemote)
            {
                return Ok(Node.QoE);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
