using Newtonsoft.Json;
using SharedHost.Models.Auth;
using RestSharp;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using WorkerManager.Middleware;
using System.Linq;
using SharedHost;
using Microsoft.Extensions.Options;
using SharedHost.Auth;

using WorkerManager.Models;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/worker")]
    [Produces("application/json")]
    public class WorkerController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ILocalStateStore _cache;

        private readonly ClusterConfig _config;

        private readonly IClusterInfor _infor;

        private readonly IPortProxy _port;

        private readonly ILog _log;

        public WorkerController( ITokenGenerator token,
                                ILocalStateStore cache,
                                IClusterInfor infor,
                                ILog log,
                                IPortProxy port,
                                IOptions<ClusterConfig> config)
        {
            _log = log;
            _cache = cache;
            _infor = infor;
            _config = config.Value;
            _tokenGenerator = token;
            _port = port;
        }


        [Owner]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> PostInfor([FromBody]WorkerRegisterModel model)
        {
            var Cluster = await _cache.GetClusterInfor();
            if(Cluster == null) { return BadRequest(); }

            var cachednode = Cluster.WorkerNodes
                .Where(x => x.model.GPU == model.GPU &&
                            x.model.RAMcapacity == model.RAMcapacity &&
                            x.model.OS  == model.OS &&
                            x.model.User == model.User &&
                            x.model.Name == model.Name);

            if(!cachednode.Any())
            {
                var client = new RestClient();
                var request = new RestRequest(_config.WorkerRegisterUrl)
                    .AddHeader("Authorization",Cluster.ClusterToken)
                    .AddJsonBody(model);
                request.Method = Method.POST;

                var result = await client.ExecuteAsync(request);
                if(result.StatusCode == HttpStatusCode.OK)
                {
                    int id = JsonConvert.DeserializeObject<int>(result.Content);
                    var node = new ClusterWorkerNode
                    {
                        ID = id,
                        model = model 
                    };

                    await _cache.CacheWorkerInfor(node);
                    await _cache.SetWorkerState(node.ID, WorkerState.Open);
                    var Token = await _tokenGenerator.GenerateWorkerToken(node);
                    return Ok(AuthResponse.GenerateSuccessful(null,Token,null));
                }
                else
                {
                    _log.Information("Fail to register device");
                    return BadRequest();
                }
            }
            else
            {
                var node = cachednode.First();
                node.model.AgentUrl = model.AgentUrl;
                await _cache.CacheWorkerInfor(node);
                var result = await _tokenGenerator.GenerateWorkerToken(cachednode.First());
                return Ok(AuthResponse.GenerateSuccessful(null,result,null));
            }
        }

        [Worker]
        [Route("session/end")]
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var workerID = Int32.Parse((string)HttpContext.Items["WorkerID"]);
            var state = await _cache.GetWorkerState(workerID);
            if(state == WorkerState.OnSession)
            {
                await _cache.SetWorkerState(workerID, WorkerState.OffRemote);
            }
            return Ok();
        }

        [Worker]
        [HttpPost("session/continue")]
        public async Task<IActionResult> shouldContinue()
        {
            var workerID = Int32.Parse((string)HttpContext.Items["WorkerID"]);

            var currentState = await _cache.GetWorkerState(workerID);
            return (currentState == WorkerState.OnSession)? Ok() : BadRequest();
        }

        [Worker]
        [HttpPost("session/token")]
        public async Task<IActionResult> Session()
        {
            var workerID = Int32.Parse((string)HttpContext.Items["WorkerID"]);

            _log.Information("Worker node get remote token: "+ workerID);
            var remoteToken = await _cache.GetWorkerRemoteToken(workerID);

            return Ok(new AuthenticationRequest{
                token = remoteToken,
                Validator = "WorkerManager",
            });
        }

        [Route("token/validate")]
        [HttpPost]
        public async Task<IActionResult> TokenValidation(string token)
        {
            return (await _tokenGenerator.ValidateToken(token) != null) ? Ok() : BadRequest();
        }

        [Worker]
        [HttpPost("log")]
        public async Task<IActionResult> Log([FromBody] string log)
        {
            var WorkerID = Int32.Parse((string)HttpContext.Items["WorkerID"]);
            _log.Information($"Log from workernode {WorkerID} : {log}");
            return Ok();
        }
    }
}
