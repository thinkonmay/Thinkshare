using Newtonsoft.Json;
using SharedHost.Models.Auth;
using SharedHost.Models.Cluster;
using RestSharp;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using WorkerManager.Middleware;
using System.Linq;
using WorkerManager;
using Microsoft.Extensions.Options;
using SharedHost;

using WorkerManager.Models;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/agent")]
    [Produces("application/json")]
    public class AgentController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ILocalStateStore _cache;

        private readonly ClusterConfig _config;

        public AgentController( ITokenGenerator token,
                                ILocalStateStore cache,
                                IOptions<ClusterConfig> config)
        {
            _cache = cache;
            _config = config.Value;
            _tokenGenerator = token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent_register"></param>
        /// <returns></returns>
        [Owner]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> PostInfor([FromBody]WorkerRegisterModel agent_register)
        {
            var Cluster = await _cache.GetClusterInfor();
            if(Cluster == null)
            {
                return BadRequest("Cluster haven't been registered yet");
            }

            var cachednode = Cluster.WorkerNodes.Where(x => 
                x.PrivateIP == agent_register.LocalIP &&
                x.CPU == agent_register.CPU &&
                x.GPU == agent_register.GPU &&
                x.RAMcapacity == (int)agent_register.RAMcapacity &&
                x.OS == agent_register.OS);

            if(!cachednode.Any())
            {
                var client = new RestClient();
                var request = new RestRequest(_config.WorkerRegisterUrl)
                    .AddQueryParameter("ClusterName",Cluster.Name)
                    .AddJsonBody(agent_register)
                    .AddHeader("Authorization","Bearer "+ Cluster.OwnerToken);
                request.Method = Method.POST;

                var result = await client.ExecuteAsync(request);
                if(result.StatusCode == HttpStatusCode.OK)
                {
                    int id = JsonConvert.DeserializeObject<int>(result.Content);
                    var node = new ClusterWorkerNode
                    {
                        ID = id.GlobalID,
                        PrivateIP = agent_register.LocalIP,
                        Register = DateTime.Now,
                        CPU = agent_register.CPU,
                        GPU = agent_register.GPU,
                        RAMcapacity = (int)agent_register.RAMcapacity,
                        OS = agent_register.OS,
                    };

                    await _cache.CacheWorkerInfor(node);
                    await _cache.SetWorkerState(node.ID, WorkerState.Open);
                    var Token = await _tokenGenerator.GenerateWorkerToken(node);
                    return Ok(AuthResponse.GenerateSuccessful(null,Token,null));
                }
                else
                {
                    return BadRequest("Fail to register worker");
                }
            }
            else
            {
                var result = await _tokenGenerator.GenerateWorkerToken(cachednode.First());
                return Ok(AuthResponse.GenerateSuccessful(null,result,null));
            }
        }

        [Worker]
        [Route("core/end")]
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var workerID = Int32.Parse((string)HttpContext.Items["PrivateID"]);
            var state = await _cache.GetWorkerState(workerID);
            if(state == WorkerState.OnSession)
            {
                await _cache.SetWorkerState(workerID, WorkerState.OffRemote);
            }
            return Ok();
        }
    }
}
