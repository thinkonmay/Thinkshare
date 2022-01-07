using Microsoft.AspNetCore.Mvc;
using System;
using WorkerManager.Models;
using WorkerManager.Interfaces;
using System.Net;
using System.Threading.Tasks;
using WorkerManager.Middleware;
using System.Linq;
using SharedHost.Models.Auth;
using RestSharp;
using Newtonsoft.Json;
using SharedHost.Models.Cluster;
using Microsoft.Extensions.Options;
using WorkerManager;
using SharedHost;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/Owner")]
    [Produces("application/json")]
    public class OwnerController : ControllerBase
    {

        private readonly ClusterConfig _config;

        private readonly RestClient _client;

        private readonly IConductorSocket _conductor;

        private readonly IWorkerNodePool _workerNodePool;

        private ILocalStateStore _cache;

        public OwnerController( ILocalStateStore cache,
                                IConductorSocket socket,
                                IWorkerNodePool workerPool,
                                IOptions<ClusterConfig> config)
        {
            _cache = cache;
            _conductor = socket;
            _workerNodePool = workerPool;
            _config = config.Value;
            _client = new RestClient();

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var request = new RestRequest( new Uri(_config.OwnerAccountUrl))
                 .AddJsonBody(login);
            request.Method = Method.POST;

            var result = await _client.ExecuteAsync(request);
            if(result.StatusCode != HttpStatusCode.OK) {return BadRequest("Fail to connect to host");}
            var jsonresult = JsonConvert.DeserializeObject<AuthResponse>(result.Content);

            if(jsonresult.Errors == null)
            {
                var cluster = await _cache.GetClusterInfor();
                if(cluster.OwnerName == null && cluster.OwnerToken == null)
                {
                    cluster.OwnerToken = jsonresult.Token;
                    cluster.OwnerName = jsonresult.UserName;
                }
                await _cache.SetClusterInfor(cluster);
                return Ok(jsonresult.Token);
            }
            else
            {
                return BadRequest(jsonresult);
            }
        }


        [Owner]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(bool isPrivate, string ClusterName)
        {
            var cluster = await _cache.GetClusterInfor();

            var request = new RestRequest(_config.ClusterRegisterUrl)
                .AddHeader("Authorization","Bearer "+cluster.OwnerToken)
                .AddQueryParameter("ClusterName", ClusterName)
                .AddQueryParameter("Private", isPrivate.ToString());

            request.Method = Method.POST;

            var result = await _client.ExecuteAsync(request);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                cluster.Name = ClusterName;
                cluster.ClusterToken = JsonConvert.DeserializeObject<string>(result.Content);
                await _cache.SetClusterInfor(cluster);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        [Owner]
        [HttpGet("Cluster/Infor")]
        public async Task<IActionResult> Infor()
        {
            var cluster = await _cache.GetClusterInfor();

            var request = new RestRequest(_config.ClusterInforUrl)
                .AddHeader("Authorization","Bearer "+cluster.OwnerToken)
                .AddQueryParameter("ClusterName", cluster.Name);
            request.Method = Method.GET;

            var result = await _client.ExecuteAsync(request);
            return Ok(JsonConvert.DeserializeObject<GlobalCluster>(result.Content));
        }



        [Owner]
        [HttpGet("Worker/State")]
        public async Task<IActionResult> clusterState()
        {
            var result = await _cache.GetClusterState();
            return Ok(result);
        }





        [Owner]
        [HttpPost("Cluster/Start")]
        public async Task<IActionResult> Start()
        {
            if(await _conductor.Start())
            {
                if(_workerNodePool.Start())
                {
                    return Ok();
                }
            }
            return BadRequest("Cluster has already been initialized");
        }


        [Owner]
        [HttpPost("Cluster/Stop")]
        public async Task<IActionResult> Stop()
        {
            if(await _conductor.Stop())
            {
                if(_workerNodePool.Stop())
                {
                    return Ok();
                }
            }
            return BadRequest("Cluster has already been initialized");
        }







        [Owner]
        [HttpPost("Cluster/isRegistered")]
        public async Task<IActionResult> isRegistered()
        {
            return ((await _cache.GetClusterInfor()).ClusterToken == null) ? BadRequest() : Ok();
        }


        [Owner]
        [HttpPost("Cluster/Worker/Infor")]
        public async Task<IActionResult> GetWorkerInfor(int ID)
        {
            var result = await _cache.GetWorkerInfor(ID);
            return (result != null) ? Ok(result) : BadRequest("Worker not found");
        }



        [Owner]
        [HttpGet("Cluster/Worker/Log")]
        public async Task<IActionResult> GetLog(int WorkerID)
        {
            return Ok(await _cache.GetLog(WorkerID,null,null));
        }

        [Owner]
        [HttpGet("Cluster/Worker/Log/Timestamp")]
        public async Task<IActionResult> GetLogWithOptions(int WorkerID, DateTime From, DateTime To)
        {
            var array = await _cache.GetLog(WorkerID,From,To);
            return (array.Count() > 60) ? Ok(array.Take(60)) : Ok(array);
        }
    }
}
