using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WorkerManager.Interfaces;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost;
using WorkerManager.Middleware;
using DbSchema.SystemDb;
using SharedHost.Models.Device;
using DbSchema.CachedState;
using System;
using RestSharp;
using Newtonsoft.Json;
using DbSchema.LocalDb;

// TODO: authentification

namespace WorkerManager.Controllers
{
    [Route("/session")]
    [ApiController]
    [Produces("application/json")]
    public class CoreController : Controller
    {
        private readonly ClusterDbContext _db;

        private readonly ILocalStateStore _cache;

        private readonly RestClient _sessionClient;

        public CoreController(IOptions<ClusterConfig> config,
                              ClusterDbContext db,
                              ILocalStateStore cache)
        {
            _db = db;
        }

        [Worker]
        [HttpGet("token")]
        public async Task<IActionResult> Session()
        {
            var PrivateID = Int32.Parse((string)HttpContext.Items["PrivateID"]);
            var Node = _db.Devices.Find(PrivateID);

            return Ok(Node.RemoteToken);
        }
    }
}
