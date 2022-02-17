using Microsoft.AspNetCore.Mvc;
using SharedHost.Auth;
using SharedHost.Models.Cluster;
using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using SharedHost;
using DbSchema.CachedState;
using System;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using SharedHost.Logging;



namespace Conductor.Controllers
{
    [ApiController]
    [Route("/Session")]
    [Produces("application/json")]
    public class SessionController : Controller
    {
        private readonly GlobalDbContext _db;

        private readonly SystemConfig _config;

        private readonly IWorkerCommnader _Cluster;

        private readonly IGlobalStateStore _cache;

        private readonly ILog _log;

        private readonly IClusterRBAC _rbac;

        public SessionController(GlobalDbContext db,
                                IOptions<SystemConfig> config,
                                IWorkerCommnader slmsocket,
                                IGlobalStateStore cache,
                                IClusterRBAC rbac,
                                ILog log)
        {
            _db = db;
            _log = log;
            _rbac = rbac;
            _cache = cache;
            _Cluster = slmsocket;
            _config = config.Value;
        }

        [User]
        [HttpPost("Initialize")]
        public async Task<IActionResult> Create(int SlaveID)
        {
            var UserID = Int32.Parse((string)HttpContext.Items["UserID"]);
            var worker = _db.Devices.Find(SlaveID);

            if(!_rbac.IsAllowed(UserID,worker))
                return Unauthorized();

            var state = await _cache.GetWorkerState(SlaveID);
            if(state != WorkerState.Open)
                return BadRequest("Worker not in open state");

            /*create new session with gevin session request from user*/
            var sess = new RemoteSession()
            {
                ClientId = UserID,
                WorkerID = SlaveID 
            };


            _db.RemoteSessions.Add(sess);
            await _db.SaveChangesAsync();

            /*generate rest post to signalling server*/
            var workerTokenRequest = new RestRequest(new Uri(_config.SessionTokenGrantor))
                .AddJsonBody(new SessionAccession
                {
                    ClientID = UserID,
                    WorkerID = sess.WorkerID,
                    ID = sess.ID,
                    Module = Module.CORE_MODULE
                });

            var clientTokenRequest = new RestRequest(new Uri(_config.SessionTokenGrantor))
                .AddJsonBody(new SessionAccession
                {
                    ClientID = UserID,
                    WorkerID = sess.WorkerID,
                    ID = sess.ID,
                    Module = Module.CLIENT_MODULE
                });

            // return bad request if fail to delete session pair      
            var clientToken = JsonConvert.DeserializeObject<AuthenticationRequest>((new RestClient()).Post(clientTokenRequest).Content);
            var workerToken = JsonConvert.DeserializeObject<AuthenticationRequest>((new RestClient()).Post(workerTokenRequest).Content);

            /*create session from client device capability*/
            var userSetting = await _cache.GetUserSetting(UserID);
            await _cache.SetSessionSetting(sess.ID,userSetting,_config, worker);

            _log.Information("Remote session between user "+UserID.ToString()+" and worker "+SlaveID+" reconnected");
            _Cluster.SessionInitialize(SlaveID, workerToken.token);
            return (await _Cluster.WaitForDesiredState(SlaveID,WorkerState.OnSession)) ? Ok(clientToken) : BadRequest();
        }


    

        [User]
        [HttpDelete("Terminate")]
        public async Task<IActionResult> Terminate(int SlaveID)
        {
            var UserID = Int32.Parse((string)HttpContext.Items["UserID"]);

            // get session information in database
            var ses = _db.RemoteSessions.Where(s => s.WorkerID == SlaveID && 
                                               s.ClientId == UserID && 
                                               s.StartTime.HasValue &&
                                              !s.EndTime.HasValue);

            // return badrequest if session is not available in database
            if (!ses.Any()) 
                return BadRequest();


            string workerState = await _cache.GetWorkerState(SlaveID);
            if(workerState != WorkerState.OnSession || workerState != WorkerState.OffRemote)
                return BadRequest("State conflict");            

            _log.Information($"Terminate remote session {ses.First().ID}");
            _Cluster.SessionTerminate(ses.First().WorkerID);
            return (await _Cluster.WaitForDesiredState(SlaveID,WorkerState.Open)) ? Ok() : BadRequest();
        }


        [User]
        [HttpPost("Disconnect")]
        public async Task<IActionResult> DisconnectRemoteControl(int SlaveID)
        {
            // get ClientId from request         
            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());

            // get session from database
            var ses = _db.RemoteSessions.Where(s => s.WorkerID == SlaveID && 
                                               s.ClientId == UserID &&
                                               s.StartTime.HasValue &&
                                              !s.EndTime.HasValue);



            // return bad request if session is not found in database
            if (!ses.Any()) 
                return BadRequest();

            /*slavepool send terminate session signal*/
            var workerState = await _cache.GetWorkerState(ses.First().WorkerID);
            if (workerState != WorkerState.OnSession)
                return BadRequest("Device not in session");            

            _log.Information($"Disconnect remote session {ses.First().ID}");
            _Cluster.SessionDisconnect(SlaveID);
            return (await _Cluster.WaitForDesiredState(SlaveID,WorkerState.OffRemote)) ? Ok() : BadRequest();
        }

        [User]
        [HttpPost("Reconnect")]
        public async Task<IActionResult> ReconnectRemoteControl(int SlaveID)
        {
            // get ClientId from user request
            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());

            // get session from database
            var ses = _db.RemoteSessions.Where(s => s.WorkerID == SlaveID && 
                                               s.ClientId == UserID && 
                                               s.StartTime.HasValue &&
                                              !s.EndTime.HasValue);

            if (!ses.Any())  
                return BadRequest(); 

            string workerState = await _cache.GetWorkerState(ses.First().WorkerID);
            if (workerState != WorkerState.OffRemote)
                return BadRequest("Device not in off remote");            

            var userSetting = await _cache.GetUserSetting(UserID);
            var clientTokenRequest = new RestRequest(new Uri(_config.SessionTokenGrantor))
                .AddJsonBody(new SessionAccession
                {
                    ClientID = UserID,
                    WorkerID = SlaveID,
                    ID = ses.First().ID,
                    Module = Module.CLIENT_MODULE
                });

            // return bad request if fail to delete session pair      
            var clientToken = JsonConvert.DeserializeObject<AuthenticationRequest>((new RestClient()).Post(clientTokenRequest).Content);

            _log.Information($"Remote session {ses.First().ID} reconnected");
            _Cluster.SessionReconnect(ses.First().WorkerID);
            return (await _Cluster.WaitForDesiredState(SlaveID,WorkerState.OnSession)) ? Ok(clientToken) : BadRequest();
        }



        
        [HttpGet("Setting")]
        public async Task<IActionResult> GetSetting(string token)
        {

            var request = new RestRequest(_config.SessionTokenValidator)
                .AddQueryParameter("token", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            if (result.StatusCode != HttpStatusCode.OK)
                return BadRequest("Token is invalid");

            var accession = JsonConvert.DeserializeObject<SessionAccession>(result.Content);
            if(accession.Module == Module.CLIENT_MODULE)
            {
                var clientSession = await _cache.GetClientSessionSetting(accession);
                _log.Information($"Got Session setting request from client{JsonConvert.SerializeObject(clientSession)}");
                return Ok(clientSession);
            }
            else if(accession.Module == Module.CORE_MODULE)
            {
                var workerSession = await _cache.GetWorkerSessionSetting(accession);
                _log.Information($"Got Session setting request from worker {JsonConvert.SerializeObject(workerSession)}");
                return Ok(workerSession);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("Setting")]
        public async Task<IActionResult> SetSetting(string token, 
                                                    [FromBody] UserSetting setting)
        {

            var request = new RestRequest(_config.SessionTokenValidator)
                .AddQueryParameter("token", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            if (result.StatusCode != HttpStatusCode.OK)
                return BadRequest("Token is invalid");

            var accession = JsonConvert.DeserializeObject<SessionAccession>(result.Content);
            var worker = _db.Devices.Find(accession.WorkerID);
            await _cache.SetSessionSetting(accession.ID,setting,_config, worker);
            return Ok();
        }
    }
}