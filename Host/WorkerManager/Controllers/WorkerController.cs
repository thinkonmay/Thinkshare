﻿using Newtonsoft.Json;
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

        public WorkerController( ITokenGenerator token,
                                ILocalStateStore cache,
                                IClusterInfor infor,
                                IOptions<ClusterConfig> config)
        {
            _cache = cache;
            _infor = infor;
            _config = config.Value;
            _tokenGenerator = token;
        }


        [Owner]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> PostInfor([FromBody]WorkerRegisterModel model)
        {
            var Cluster = await _cache.GetClusterInfor();



            if(Cluster == null) { return BadRequest(); }

            var cachednode = Cluster.WorkerNodes.Where(x => x.model == model);

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
                    Serilog.Log.Information("Fail to register device");
                    return BadRequest();
                }
            }
            else
            {
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

            Serilog.Log.Information("Worker node get remote token: "+ workerID);
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


            var cluster = await _cache.GetClusterInfor();
            var worker = cluster.WorkerNodes.Where(x => x.ID == WorkerID).First();
            await _cache.Log(new Log
            {
                WorkerID = worker.ID,
                Content = log,
                LogTime = DateTime.Now,
            });
            return Ok();
        }
    }
}