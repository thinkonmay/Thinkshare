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
            var globalCluster = _db.Clusters.Where(x => x.WorkerNode.Contains(worker)).First();
            var userSetting = await _cache.GetUserSetting(UserID);
            await _cache.SetSessionSetting(sess.ID,userSetting,_config, globalCluster);

            // invoke session initialization in slave pool
            await _Cluster.SessionInitialize(SlaveID, workerToken.token);

            // return view for user
            _log.Information("Remote session between user "+UserID.ToString()+" and worker "+SlaveID+" reconnected");
            return Ok(clientToken);
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
            if (!ses.Any()) return BadRequest();


            string workerState = await _Cluster.GetWorkerState(SlaveID);
            /*slavepool send terminate session signal*/
            if(workerState == WorkerState.OnSession
            || workerState == WorkerState.OffRemote)
            {
                //
                _log.Information($"Terminate remote session {ses.First().ID}");
                await _Cluster.SessionTerminate(ses.First().WorkerID);
                return Ok();
            }
            return BadRequest("Cannot send terminate session signal to slave");            
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
                                              !s.EndTime.HasValue).FirstOrDefault();



            // return bad request if session is not found in database
            if (ses == null) return BadRequest();
            var workerState = await _Cluster.GetWorkerState(ses.WorkerID);
            /*slavepool send terminate session signal*/
            if (workerState == WorkerState.OnSession)
            {
                // send disconnect signal to slave
                _log.Information($"Disconnect remote session {ses.ID}");
                await _Cluster.SessionDisconnect(SlaveID);
                return Ok();
            }
            return BadRequest("Device not in session");            
        }

        [User]
        [HttpPost("Reconnect")]
        public async Task<IActionResult> ReconnectRemoteControl(int SlaveID)
        {
            // get ClientId from user request
            var UserID = HttpContext.Items["UserID"];

            // get session from database
            var ses = _db.RemoteSessions.Where(s => s.WorkerID == SlaveID && 
                                               s.ClientId == Int32.Parse(UserID.ToString())&& 
                                               s.StartTime.HasValue &&
                                              !s.EndTime.HasValue);


            if (!ses.Any()) { return BadRequest(); }
            var userSetting = await _cache.GetUserSetting(Int32.Parse((string)UserID));
            var clientTokenRequest = new RestRequest(new Uri(_config.SessionTokenGrantor))
                .AddJsonBody(new SessionAccession
                {
                    ClientID = Int32.Parse((string)UserID),
                    WorkerID = SlaveID,
                    ID = ses.First().ID,
                    Module = Module.CLIENT_MODULE
                });

            // return bad request if fail to delete session pair      
            var clientToken = JsonConvert.DeserializeObject<AuthenticationRequest>((new RestClient()).Post(clientTokenRequest).Content);

            // return null if session is not found
            if (ses == null) return BadRequest();

            string workerState = await _Cluster.GetWorkerState(ses.First().WorkerID);

            /*slavepool send terminate session signal*/
            if (workerState == WorkerState.OffRemote)
            {
                // reconect remote control
                await _Cluster.SessionReconnect(ses.First().WorkerID);
                _log.Information($"Remote session {ses.First().ID} reconnected");

                // return token to client 
                return Ok(clientToken);
            }
            return BadRequest("Device not in off remote");            
        }



        
        [HttpGet("Setting")]
        public async Task<IActionResult> GetSetting(string token)
        {

            var request = new RestRequest(_config.SessionTokenValidator)
                .AddQueryParameter("token", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var accession = JsonConvert.DeserializeObject<SessionAccession>(result.Content);
                if(accession.Module == Module.CLIENT_MODULE)
                {
                    var clientSession = await _cache.GetClientSessionSetting(accession);
                    _log.Information("Got Session setting request from client");
                    _log.Information("Result "+JsonConvert.SerializeObject(clientSession));
                    return Ok(clientSession);
                }
                else
                {
                    var workerSession = await _cache.GetWorkerSessionSetting(accession);
                    _log.Information("Got Session setting request from worker");
                    _log.Information("Result: "+JsonConvert.SerializeObject(workerSession));
                    return Ok(workerSession);
                }
            }
            else
            {
                _log.Information("Fail to parse token");
                return BadRequest("Token is invalid");
            }
        }

        [HttpPost("Setting")]
        public async Task<IActionResult> SetSetting(string token, [FromBody]UserSetting setting)
        {

            var request = new RestRequest(_config.SessionTokenValidator)
                .AddQueryParameter("token", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var accession = JsonConvert.DeserializeObject<SessionAccession>(result.Content);
                var worker = _db.Devices.Find(accession.WorkerID);
                var cluster = _db.Clusters.Where(o=>o.WorkerNode.Contains(worker)).First();
                await _cache.SetSessionSetting(accession.ID,setting,_config, cluster);
                return Ok();
            }
            else
            {
                _log.Information("Fail to parse token");
                return BadRequest("Token is invalid");
            }
        }


        [HttpPost("Parsec")]
        public async Task<IActionResult> SetParsec(string token)
        {

            var request = new RestRequest($"{_config.SessionTokenValidator}",Method.POST)
                .AddQueryParameter("token", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var accession = JsonConvert.DeserializeObject<SessionAccession>(result.Content);
                return Ok(await _cache.GetParsecCred(accession));
            }
            else
            {
                _log.Information("Fail to parse token");
                return BadRequest("Token is invalid");
            }
        }
    }
}