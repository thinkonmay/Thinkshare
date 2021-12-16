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
using DbSchema.CachedState;
using SharedHost;
using DbSchema.LocalDb;
using DbSchema.LocalDb.Models;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/Owner")]
    [Produces("application/json")]
    public class OwnerController : ControllerBase
    {

        private readonly ClusterConfig _config;

        private readonly ClusterDbContext _db;

        private readonly RestClient _client;

        private readonly IConductorSocket _conductor;

        private readonly IWorkerNodePool _workerNodePool;

        private ILocalStateStore _cache;

        public OwnerController(ClusterDbContext db, 
                                ILocalStateStore cache,
                                IConductorSocket socket,
                                IWorkerNodePool workerPool,
                                IOptions<ClusterConfig> config)
        {
            _db = db;
            _cache = cache;
            _conductor = socket;
            _workerNodePool = workerPool;
            _config = config.Value;
            _client = new RestClient();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
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
                if(_db.Owner.Any())
                {
                    var owner = _db.Owner.First();
                    if(owner.Name == jsonresult.UserName)
                    {
                        owner.token = jsonresult.Token;
                        _db.Update(owner);
                        await _db.SaveChangesAsync();
                        return Ok(jsonresult.Token);
                    }
                    else
                    {
                        return BadRequest("Wrong user");
                    }
                }
                else
                {
                    _db.Owner.Add(new OwnerCredential 
                    {   Name = jsonresult.UserName, 
                        Description = "Primary owner of the cluster", 
                        token = jsonresult.Token
                    });
                    await _db.SaveChangesAsync();
                    return Ok(jsonresult.Token);
                }
            }
            else
            {
                return BadRequest(jsonresult);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isPrivate"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(bool isPrivate, string ClusterName)
        {
            if(_db.Clusters.Any())
            {
                return BadRequest("cluster is already registered");
            }


            var token = _db.Owner.First().token;
            var request = new RestRequest(_config.ClusterRegisterUrl)
                .AddHeader("Authorization","Bearer "+token)
                .AddQueryParameter("ClusterName", ClusterName)
                .AddQueryParameter("Private", isPrivate.ToString());

            request.Method = Method.POST;

            var result = await _client.ExecuteAsync(request);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var cluster = new LocalCluster
                {
                    Name = ClusterName,
                    Token = result.Content,
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
        [HttpGet("Cluster/Infor")]
        public async Task<IActionResult> Infor()
        {
            var ClusterName = _db.Clusters.First().Name;
            var token = _db.Owner.First().token;
            var request = new RestRequest(_config.ClusterInforUrl)
                .AddHeader("Authorization","Bearer "+token)
                .AddQueryParameter("ClusterName", ClusterName);
            request.Method = Method.GET;

            var result = await _client.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK)            
            {
                return Ok(result.Content);
            }
            else
            {
                return BadRequest(result.Content);
            }
        }



        [Owner]
        [HttpGet("Cluster/Token")]
        public async Task<IActionResult> Token()
        {
            var ClusterName = _db.Clusters.First().Name;
            var token = _db.Owner.First().token;
            var request = new RestRequest(_config.ClusterTokenUrl)
                .AddHeader("Authorization","Bearer "+token)
                .AddQueryParameter("ClusterName", ClusterName);
            request.Method = Method.GET;

            var result = await _client.ExecuteAsync(request);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                if(_db.Clusters.Any())
                {
                    var cluster = _db.Clusters.First();
                    cluster.Token = JsonConvert.DeserializeObject<string>(result.Content);
                    _db.Update(cluster);
                    await _db.SaveChangesAsync();
                }
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [Owner]
        [HttpGet("Worker/State")]
        public async Task<IActionResult> clusterState()
        {
            var result = await _cache.GetClusterState();
            return Ok(result);
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("Cluster/TURN")]
        public async Task<IActionResult> setturn(string turnIP, 
                                                 string turnUSER, 
                                                 string turnPASSWORD)
        {
            var cluster = _db.Clusters.First();
            var owner = _db.Owner.First();

            var request = new RestRequest(_config.ClusterTURNUrl)
                .AddHeader("Authorization","Bearer "+owner.token)
                .AddQueryParameter("ClusterName",cluster.Name)
                .AddQueryParameter("turnIP",turnIP)
                .AddQueryParameter("turnUSER",turnUSER)
                .AddQueryParameter("turnPASSWORD",turnPASSWORD);
            request.Method = Method.POST;


            var result = await _client.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Fail to update cluster turn");
            }
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
            if(_db.Clusters.Any())
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
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
            var worker = _db.Devices.Find(WorkerID);
            return Ok(worker.Logs);
        }

        [Owner]
        [HttpGet("Cluster/Worker/Log/Timestamp")]
        public async Task<IActionResult> GetLog(int WorkerID, DateTime From, DateTime To)
        {
            var worker = _db.Devices.Find(WorkerID);
            return Ok(worker.Logs.Where(x => x.LogTime > From && x.LogTime < To));
        }
    }
}
