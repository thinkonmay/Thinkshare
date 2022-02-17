using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using WorkerManager.Interfaces;
using System.Net;
using System.Threading.Tasks;
using WorkerManager.Middleware;
using System.Linq;
using System.Collections.Generic;
using SharedHost.Models.Auth;
using RestSharp;
using Newtonsoft.Json;
using SharedHost.Models.Cluster;
using Microsoft.Extensions.Options;
using SharedHost;
using SharedHost.Auth;

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

        private IClusterInfor _infor;

        private IPortProxy _port;

        public OwnerController( ILocalStateStore cache,
                                IConductorSocket socket,
                                IWorkerNodePool workerPool,
                                IPortProxy port,
                                IClusterInfor infor,
                                IOptions<ClusterConfig> config)
        {
            _cache = cache;
            _infor = infor;
            _port = port;
            _conductor = socket;
            _workerNodePool = workerPool;
            _config = config.Value;
            _client = new RestClient();

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string? ClusterName, 
                                              [FromBody] LoginModel login)
        {
            bool registered = await _infor.IsRegistered();

            if(!registered && ClusterName == null) 
                return BadRequest("Cluster haven't been registered");

            var loginResult = await _client.ExecuteAsync(
                new RestRequest($"https://{_config.Domain}{_config.OwnerAccountUrl}",Method.POST)
                    .AddJsonBody(login));

            if(loginResult.StatusCode != HttpStatusCode.OK) {return BadRequest();}
            var jsonresult = JsonConvert.DeserializeObject<AuthResponse>(loginResult.Content);

            if(jsonresult.Errors == null)
            {
                var cluster = await _cache.GetClusterInfor();

                if(!registered)
                {
                    cluster.OwnerToken = jsonresult.Token;
                    cluster.OwnerName = jsonresult.UserName;
                    var tokenResult = await _client.ExecuteAsync( 
                        new RestRequest($"https://{_config.Domain}{_config.ClusterRegisterUrl}",Method.GET)
                            .AddHeader("Authorization",cluster.OwnerToken)
                            .AddQueryParameter("region", ClusterName)
                            .AddQueryParameter("ClusterName", ClusterName));
                    if (tokenResult.StatusCode == HttpStatusCode.OK)
                    {
                        cluster.ClusterToken = JsonConvert.DeserializeObject<AuthenticationRequest>(tokenResult.Content).token;
                        await _cache.SetClusterInfor(cluster);
                        await _conductor.Start();
                        await _port.Start();
                    }
                }
                else
                {
                    if(cluster.OwnerName != jsonresult.UserName)
                    {
                        jsonresult.Errors.Add(new IdentityError{
                            Code = "Unauthorized",
                            Description = "Registered account does not the owner of this cluster",
                        });
                    }
                }
            }
            return Ok(jsonresult);
        }

        [Owner]
        [HttpGet("Cluster/Infor")]
        public async Task<IActionResult> Infor()
        {
            var cluster = await _infor.Infor();
            return Ok(cluster);
        }

        [HttpPost("Cluster/isRegistered")]
        public async Task<IActionResult> isRegistered()
        {
            return Ok(await _infor.IsRegistered());
        }

        [Owner]
        [HttpGet("Worker/State")]
        public async Task<IActionResult> clusterState()
        {
            var result = await _cache.GetClusterState();
            return Ok(result);
        }

        [Owner]
        [HttpDelete("Cluster/Unregister")]
        public async Task<IActionResult> Unregister()
        {
            var key = await _cache.GetClusterInfor();
            var request = new RestRequest(
                $"https://{_config.Domain}{_config.UnregisterURL}",Method.DELETE)
                    .AddHeader("Authorization",key.ClusterToken);
            var restResponse = await _client.ExecuteAsync(request);
            return restResponse.StatusCode == HttpStatusCode.OK ? Ok() : BadRequest(restResponse.Content);
        }





















        [Owner]
        [HttpPost("Cluster/Role")]
        public async Task<IActionResult> AddRole(DateTime? Start,
                                                 DateTime? End,
                                                 string UserName)
        {
            var cluster = await _cache.GetClusterInfor();
            var request = new RestRequest($"https://{_config.Domain}{_config.ClusterRole}", Method.POST)
                .AddHeader("Authorization",cluster.ClusterToken)
                .AddJsonBody(new ClusterRoleRequest
                {
                    Start = Start,
                    Endtime = End,
                    User = UserName,
                    Description = "Created by cluster owner"
                });

            var result = await _client.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK) ? Ok() : BadRequest();
        }

        [Owner]
        [HttpDelete("Cluster/Role")]
        public async Task<IActionResult> RemoveRole(int RoleID)
        {
            var cluster = await _cache.GetClusterInfor();
            var request = new RestRequest($"https://{_config.Domain}{_config.ClusterRole}",Method.DELETE)
                .AddQueryParameter("RoleID",RoleID.ToString())
                .AddHeader("Authorization",cluster.ClusterToken);

            var result = await _client.ExecuteAsync(request);
            return result.StatusCode == HttpStatusCode.OK ? Ok() : BadRequest();
        }

        [Owner]
        [HttpGet("Cluster/Role")]
        public async Task<IActionResult> ClusterRole()
        {
            var cluster = await _cache.GetClusterInfor();
            var request = new RestRequest($"https://{_config.Domain}{_config.ClusterRole}",Method.GET)
                .AddHeader("Authorization",cluster.ClusterToken);

            var result = await _client.ExecuteAsync(request);
            return Ok(JsonConvert.DeserializeObject<List<ClusterRole>>(result.Content));
        }
    }
}
