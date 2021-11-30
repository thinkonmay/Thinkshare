using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using SharedHost.Models.Cluster;
using RestSharp;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using DbSchema.SystemDb;
using WorkerManager.Middleware;
using System.Linq;
using DbSchema.CachedState;
using SharedHost.Models.Local;
using MersenneTwister;
using Microsoft.Extensions.Options;
using SharedHost;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/Agent")]
    [Produces("application/json")]
    public class AgentController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ClusterDbContext _db;

        private readonly LocalStateStore _cache;

        private readonly ClusterConfig _config;

        public AgentController( ClusterDbContext db, 
                                ITokenGenerator token,
                                LocalStateStore cache,
                                IOptions<ClusterConfig> config)
        {
            _cache = cache;
            _db = db;
            _config = config.Value;
            _tokenGenerator = token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent_register"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("Register")]
        public async Task<IActionResult> PostInfor([FromBody]WorkerRegisterModel agent_register)
        {
            var cachednode = _db.Devices.Where(x => 
                x.PrivateIP == agent_register.LocalIP &&
                x.CPU == agent_register.CPU &&
                x.GPU == agent_register.GPU &&
                x.RAMcapacity == (int)agent_register.RAMcapacity &&
                x.OS == agent_register.OS );
            if(!cachednode.Any())
            {
                var client = new RestClient();
                var request = new RestRequest(_config.WorkerRegisterUrl)
                    .AddQueryParameter("ClusterName",_config.ClusterName)
                    .AddJsonBody(agent_register)
                    .AddHeader("Authorization","Bearer "+_db.Owner.First().token);

                var result = await client.ExecuteAsync(request);
                if(result.StatusCode == HttpStatusCode.OK)
                {
                    IDAssign id = JsonConvert.DeserializeObject<IDAssign>(result.Content);
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

                    _db.Devices.Add(node);
                    await _db.SaveChangesAsync();
                    await _cache.CacheWorkerInfor(node);
                    await _cache.SetWorkerState(node.ID, WorkerState.Open);
                    return Ok(await _tokenGenerator.GenerateWorkerToken(node));
                }
                else
                {
                    return BadRequest("Fail to register worker");
                }
            }
            else
            {
                return Ok(await _tokenGenerator.GenerateWorkerToken(cachednode.First()));
            }
        }

        [Worker]
        [HttpPost("EndRemote")]
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
