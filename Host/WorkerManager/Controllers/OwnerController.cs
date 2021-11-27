using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using WorkerManager.Services;
using System;
using System.Net;
using System.Threading.Tasks;
using WorkerManager.Data;
using WorkerManager.Middleware;
using System.Linq;
using SharedHost.Models.Auth;
using RestSharp;
using Newtonsoft.Json;
using SharedHost.Models.Cluster;
using SharedHost.Models.Local;
using DbSchema.CachedState;
using SharedHost;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/Owner")]
    [Produces("application/json")]
    public class OwnerController : ControllerBase
    {

        private readonly ClusterConfig _config;

        private readonly ClusterDbContext _db;

        private readonly RestClient _login;

        private readonly RestClient _cluster;

        private readonly IConductorSocket _conductor;

        private readonly IWorkerNodePool _workerNodePool;

        private ILocalStateStore _cache;

        public OwnerController(ClusterDbContext db, 
                                ILocalStateStore cache,
                                IConductorSocket socket,
                                IWorkerNodePool workerPool,
                                ClusterConfig config)
        {
            _db = db;
            _cache = cache;
            _conductor = socket;
            _workerNodePool = workerPool;
            _config = config;
            _login = new RestClient("https://"+ config.HostDomain + "/Account");
            _cluster = new RestClient("https://"+ config.HostDomain+ "/Cluster");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var request = new RestRequest("Login")
                 .AddJsonBody(login);
            request.Method = Method.POST;

            var result = await _login.ExecuteAsync(request);
            var jsonresult = JsonConvert.DeserializeObject<AuthResponse>(result.Content);

            if(jsonresult.Errors == null)
            {
                if(_db.Owner.Any())
                {
                    if(_db.Owner.FirstOrDefault().Name == jsonresult.UserName)
                    {
                        return Ok(jsonresult.Token);
                    }
                    else
                    {
                        return BadRequest("Wrong user");
                    }
                }
                _db.Owner.Add(new OwnerCredential 
                {   Name = jsonresult.UserName, 
                    Description = "Primary owner of the cluster", 
                    token = jsonresult.Token
                });
                await _db.SaveChangesAsync();
                return Ok(jsonresult.Token);
            }
            else
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isPrivate"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(bool isPrivate)
        {
            var request = new RestRequest("Register")
                .AddQueryParameter("ClusterName", _config.ClusterName)
                .AddQueryParameter("Private", isPrivate.ToString());

            var token = _db.Owner.First().token;

            request.AddHeader("Authorization","Bearer "+token);
            request.Method = Method.POST;

            var result = await _cluster.ExecuteAsync(request);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var cluster = new LocalCluster
                {
                    Token = result.Content,
                    Private = isPrivate,
                    Register = DateTime.Now
                };

                _db.Clusters.Add(cluster);
                await _db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }





        [Owner]
        [HttpGet("GetToken")]
        public async Task<IActionResult> Token()
        {
            var token = _db.Owner.First().token;
            var request = new RestRequest("Token")
                .AddQueryParameter("ClusterName", _config.ClusterName);

            request.AddHeader("Authorization","Bearer "+token);
            request.Method = Method.GET;

            var result = await _cluster.ExecuteAsync(request);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var cluster = _db.Clusters.First();
                cluster.Token = JsonConvert.DeserializeObject<string>(result.Content);
                _db.Update(cluster);
                await _db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("SetTURN")]
        public async Task<IActionResult> setturn(string IP, string user, string password)
        {
            var cluster = _db.Clusters.First();
            cluster.TurnIP = IP;
            cluster.TurnUser = user;
            cluster.TurnPassword = password;
            _db.Update(cluster);
            await _db.SaveChangesAsync();




            return Ok();
        }


        [Owner]
        [HttpPost("Start")]
        public async Task<IActionResult> Start()
        {
            if(!_conductor.Initialized && !_workerNodePool.Initialized)
            {
                if(await _conductor.Start())
                {
                    _workerNodePool.Start();
                    return Ok();
                }
                else
                {
                    return BadRequest("Fail to connect to system hub");

                }
            }
            return BadRequest("Cluster has already been initialized");
        }

    }
}
