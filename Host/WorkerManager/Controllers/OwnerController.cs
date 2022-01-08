using Microsoft.AspNetCore.Mvc;
using System;
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
        public async Task<IActionResult> Login(string? ClusterName, [FromBody] LoginModel login)
        {
            var loginResult = await _client.ExecuteAsync(
                new RestRequest( _config.OwnerAccountUrl,Method.POST)
                    .AddJsonBody(login));

            if(loginResult.StatusCode != HttpStatusCode.OK) {return BadRequest();}
            var jsonresult = JsonConvert.DeserializeObject<AuthResponse>(loginResult.Content);

            if(jsonresult.Errors == null)
            {
                var cluster = await _cache.GetClusterInfor();
                if(cluster.OwnerName == null && cluster.OwnerToken == null)
                {
                    cluster.OwnerToken = jsonresult.Token;
                    cluster.OwnerName = jsonresult.UserName;
                }

                if (ClusterName == null)
                {
                    return Ok(jsonresult);
                }
                
                var tokenResult = await _client.ExecuteAsync( 
                    new RestRequest(_config.ClusterRegisterUrl,Method.POST)
                        .AddHeader("Authorization",cluster.OwnerToken)
                        .AddQueryParameter("ClusterName", ClusterName));

                if (tokenResult.StatusCode == HttpStatusCode.OK)
                {
                    cluster.ClusterToken = JsonConvert.DeserializeObject<string>(tokenResult.Content);
                }
                await _cache.SetClusterInfor(cluster);
            }

            return Ok(jsonresult);
        }


        [Owner]
        [HttpGet("Cluster/Infor")]
        public async Task<IActionResult> Infor()
        {
            var cluster = await _cache.GetClusterInfor();

            var request = new RestRequest(_config.ClusterInforUrl)
                .AddHeader("Authorization",cluster.ClusterToken);
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
            return Ok((await _cache.GetClusterInfor()).ClusterToken == null);
        }


        [Owner]
        [HttpPost("Cluster/Worker/Infor")]
        public async Task<IActionResult> GetWorkerInfor(int ID)
        {
            var result = await _cache.GetWorkerInfor(ID);
            return Ok(result);
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
