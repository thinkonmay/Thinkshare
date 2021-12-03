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
using SharedHost.Models.Cluster;
using System;
using RestSharp;
using Newtonsoft.Json;
using DbSchema.LocalDb;
using DbSchema.LocalDb.Models;

// TODO: authentification

namespace WorkerManager.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class LogController : Controller
    {
        private readonly ClusterDbContext _db;

        private readonly ILocalStateStore _cache;

        private readonly RestClient _sessionClient;

        public LogController(IOptions<ClusterConfig> config,
                              ClusterDbContext db,
                              ILocalStateStore cache)
        {
            _db = db;
        }

        [Worker]
        [HttpPost("log")]
        public async Task<IActionResult> Log([FromBody] string log)
        {
            var PrivateID = Int32.Parse((string)HttpContext.Items["PrivateID"]);
            var worker = _db.Devices.Find(PrivateID);

            var logRecord = new Log
            {
                Content = log,
                worker = worker
            };

            _db.Logs.Add(logRecord);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
