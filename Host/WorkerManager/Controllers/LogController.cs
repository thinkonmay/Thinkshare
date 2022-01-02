using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WorkerManager.Interfaces;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost;
using System.Linq;
using WorkerManager.Middleware;
using SharedHost.Models.Device;
using WorkerManager;
using SharedHost.Models.Cluster;
using System;
using RestSharp;
using Newtonsoft.Json;
using WorkerManager.Models;

// TODO: authentification

namespace WorkerManager.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class LogController : Controller
    {
        private readonly ILocalStateStore _cache;

        private readonly RestClient _sessionClient;

        public LogController(IOptions<ClusterConfig> config,
                              ILocalStateStore cache)
        {
        }


        [HttpPost("log")]
        public async Task<IActionResult> Log([FromBody] string log, string ip)
        {
            var cluster = await _cache.GetClusterInfor();
            var worker = cluster.WorkerNodes.Where(x => x.PrivateIP == ip).First();
            _cache.Log(worker.ID,new Log
            {
                WorkerID = worker.ID,
                Content = log,
                LogTime = DateTime.Now,
            });
            return Ok();
        }
    }
}
