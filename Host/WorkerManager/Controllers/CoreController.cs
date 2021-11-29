using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost;
using WorkerManager.Middleware;
using WorkerManager.Data;
using SharedHost.Models.Device;
using DbSchema.CachedState;
using System;
using RestSharp;
using Newtonsoft.Json;

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

        public CoreController(ClusterConfig config,
                              ClusterDbContext db,
                              ILocalStateStore cache)
        {
            _db = db;
        }

        [Worker]
        [HttpPost("token")]
        public async Task<IActionResult> Session()
        {
            var PrivateID = Int32.Parse((string)HttpContext.Items["PrivateID"]);
            var Node = _db.Devices.Find(PrivateID);
            var State = await _cache.GetWorkerState(PrivateID);

            if (State == WorkerState.OnSession ||
                State == WorkerState.OffRemote)
            {
                return Ok(Node.RemoteToken);
            }
            else
            {
                return BadRequest();
            }
        }















        [Worker]
        [HttpPost("infor")]
        public async Task<IActionResult> QoE()
        {
            var PrivateID = Int32.Parse((string)HttpContext.Items["PrivateID"]);
            var Node = _db.Devices.Find(PrivateID);
            var State = await _cache.GetWorkerState(PrivateID);

            if (State == WorkerState.OnSession ||
                State == WorkerState.OffRemote)
            {
                var request = new RestRequest("Setting")
                    .AddQueryParameter("token", Node.RemoteToken);
                request.Method = Method.GET;

                var result = await _sessionClient.ExecuteAsync(request);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var sessionWorker = JsonConvert.DeserializeObject<SessionWorker>(result.Content);
                    return Ok(sessionWorker);
                }
                {
                    return BadRequest("Broken session token");
                }
            }
            else
            {
                return BadRequest("receive request when device is not on session");
            }
        }
    }
}
