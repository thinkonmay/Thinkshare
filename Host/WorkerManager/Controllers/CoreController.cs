using Microsoft.AspNetCore.Mvc;
using SharedHost.Auth;
using Microsoft.Extensions.Options;
using WorkerManager.Interfaces;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost;
using WorkerManager.Middleware;
using SharedHost.Models.Device;
using WorkerManager;
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
        private readonly ILocalStateStore _cache;

        private readonly RestClient _sessionClient;

        private readonly ClusterConfig _config;

        public CoreController(IOptions<ClusterConfig> config,
                              ILocalStateStore cache)
        {
            _config = config.Value;
            _cache = cache;
        }


        [Worker]
        [HttpGet("token")]
        public async Task<IActionResult> Session()
        {
            var workerID = Int32.Parse((string)HttpContext.Items["PrivateID"]);
            Serilog.Log.Information("Worker node get remote token: "+ workerID);
            var remoteToken = await _cache.GetWorkerRemoteToken(workerID);

            return Ok(new AuthenticationRequest{
                token = remoteToken,
                Validator = "WorkerManager",
            });
        }

        [Worker]
        [HttpGet("continue")]
        public async Task<IActionResult> shouldContinue()
        {
            var workerID = Int32.Parse((string)HttpContext.Items["PrivateID"]);
            var currentState = await _cache.GetWorkerState(workerID);
            return (currentState == WorkerState.OnSession)? Ok() : BadRequest();
        }
    }
}
