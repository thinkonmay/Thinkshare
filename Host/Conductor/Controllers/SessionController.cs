﻿using Microsoft.AspNetCore.Mvc;
using SharedHost.Auth;
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
using SharedHost.Auth.ThinkmayAuthProtocol;
using DbSchema.CachedState;
using System;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace Conductor.Controllers
{
    /// <summary>
    /// Routes related to session initialize/termination process
    /// </summary>
    [ApiController]
    [Route("/Session")]
    [Produces("application/json")]
    public class SessionController : Controller
    {
        private readonly GlobalDbContext _db;

        private readonly SystemConfig _config;

        private readonly RestClient _sessionToken;

        private readonly IWorkerCommnader _Cluster;

        private readonly UserManager<UserAccount> _userManager;

        private readonly IGlobalStateStore _cache;

        public SessionController(GlobalDbContext db,
                                IOptions<SystemConfig> config,
                                IWorkerCommnader slmsocket,
                                IGlobalStateStore cache,
                                UserManager<UserAccount> userManager)
        {
            _db = db;
            _cache = cache;
            _Cluster = slmsocket;
            _userManager = userManager;
            _config = config.Value;
            _sessionToken = new RestClient();
        }

        /// <summary>
        /// initialize session
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [User]
        [HttpPost("Initialize")]
        public async Task<IActionResult> Create(int SlaveID)
        {
            var UserID = HttpContext.Items["UserID"];
            var worker = _db.Devices.Where(x => x.ID == SlaveID).FirstOrDefault();
            var workerState = await _Cluster.GetWorkerState(SlaveID);
            // search for availability of slave device
            if (workerState != WorkerState.Open) { return BadRequest("Device Not Available"); }

            /*create new session with gevin session request from user*/
            var sess = new RemoteSession()
            {
                Client = await _userManager.FindByIdAsync(UserID.ToString()),
                Worker = _db.Devices.Find(SlaveID)
            };


            _db.RemoteSessions.Add(sess);
            await _db.SaveChangesAsync();

            /*generate rest post to signalling server*/
            var workerTokenRequest = new RestRequest(new Uri(_config.SessionTokenGrantor))
                .AddJsonBody(new SessionAccession
                {
                    ClientID = Int32.Parse((string)UserID),
                    WorkerID = sess.Worker.ID,
                    ID = sess.ID,
                    Module = Module.CORE_MODULE
                });

            var clientTokenRequest = new RestRequest(new Uri(_config.SessionTokenGrantor))
                .AddJsonBody(new SessionAccession
                {
                    ClientID = Int32.Parse((string)UserID),
                    WorkerID = sess.Worker.ID,
                    ID = sess.ID,
                    Module = Module.CLIENT_MODULE
                });

            // return bad request if fail to delete session pair      
            var clientToken = JsonConvert.DeserializeObject<AuthenticationRequest>(_sessionToken.Post(clientTokenRequest).Content);
            var workerToken = JsonConvert.DeserializeObject<AuthenticationRequest>(_sessionToken.Post(workerTokenRequest).Content);




            /*create session from client device capability*/
            var userSetting = await _cache.GetUserSetting(Int32.Parse((string)UserID));
            var globalCluster = _db.Clusters.Where(x => x.WorkerNode.Contains(sess.Worker)).First();
            await _cache.SetSessionSetting(sess.ID,userSetting,_config, globalCluster);
            // invoke session initialization in slave pool
            await _Cluster.SessionInitialize(SlaveID, workerToken.token);

            // return view for user
            Serilog.Log.Information("Remote session between user "+sess.Client.FullName+" and worker "+SlaveID+" reconnected");
            return Ok(clientToken);
        }


    

        /// <summary>
        /// Terminate session 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [User]
        [HttpDelete("Terminate")]
        public async Task<IActionResult> Terminate(int SlaveID)
        {
            var UserID = HttpContext.Items["UserID"];
            var userAccount = await _userManager.FindByIdAsync(UserID.ToString());

            var device = _db.Devices.Find(SlaveID);

            string workerState = await _Cluster.GetWorkerState(SlaveID);
            // get session information in database
            var ses = _db.RemoteSessions.Where(s => s.Worker == device && 
                                               s.Client == userAccount && 
                                              !s.EndTime.HasValue);

            // return badrequest if session is not available in database
            if (!ses.Any()) return BadRequest();


            /*slavepool send terminate session signal*/
            if(workerState == WorkerState.OnSession
            || workerState == WorkerState.OffRemote)
            {
                //
                await _Cluster.SessionTerminate(ses.First().WorkerID);
                return Ok();
            }
            Serilog.Log.Information("Start remote session between user "+userAccount.FullName+" and worker "+SlaveID);
            return BadRequest("Cannot send terminate session signal to slave");            
        }


        /// <summary>
        /// disconnect remote control during session
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [User]
        [HttpPost("Disconnect")]
        public async Task<IActionResult> DisconnectRemoteControl(int SlaveID)
        {
            // get ClientId from request         
            var UserID = HttpContext.Items["UserID"];
            var userAccount = await _userManager.FindByIdAsync(UserID.ToString());


            // get session from database
            var ses = _db.RemoteSessions.Where(s => s.WorkerID == SlaveID  
                                               && s.Client == userAccount 
                                              && !s.EndTime.HasValue).FirstOrDefault();



            // return bad request if session is not found in database
            if (ses == null) return BadRequest();
            var workerState = await _Cluster.GetWorkerState(ses.WorkerID);

            /*slavepool send terminate session signal*/
            if (workerState == WorkerState.OnSession)
            {
                // send disconnect signal to slave
                await _Cluster.SessionDisconnect(ses.Worker.ID);
                return Ok();
            }
            Serilog.Log.Information("Remote session between user "+userAccount.FullName+" and worker "+SlaveID+" disconnected");
            return BadRequest("Device not in session");            
        }

        /// <summary>
        /// Reconnect remote control after disconnect
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [User]
        [HttpPost("Reconnect")]
        public async Task<IActionResult> ReconnectRemoteControl(int SlaveID)
        {
            // get ClientId from user request
            var UserID = HttpContext.Items["UserID"];
            var userAccount = await _userManager.FindByIdAsync(UserID.ToString());

            var device = _db.Devices.Find(SlaveID);

            // get session from database
            var ses = _db.RemoteSessions.Where(s => s.Worker == device && 
                                               s.Client == userAccount && 
                                              !s.EndTime.HasValue);
            if (!ses.Any()) { return BadRequest(); }


            var userSetting = await _cache.GetUserSetting(Int32.Parse((string)UserID));


            _db.Update(ses.First());
            await _db.SaveChangesAsync();

            var clientTokenRequest = new RestRequest("GrantSession")
                .AddJsonBody(new SessionAccession
                {
                    ClientID = Int32.Parse((string)UserID),
                    WorkerID = device.ID,
                    ID = ses.First().ID,
                    Module = Module.CLIENT_MODULE
                });

            // return bad request if fail to delete session pair      
            var clientToken = JsonConvert.DeserializeObject<AuthenticationRequest>(_sessionToken.Post(clientTokenRequest).Content);

            // return null if session is not found
            if (ses == null) return BadRequest();

            string workerState = await _Cluster.GetWorkerState(ses.First().WorkerID);

            /*slavepool send terminate session signal*/
            if (workerState == WorkerState.OffRemote)
            {
                // reconect remote control
                await _Cluster.SessionReconnect(ses.First().WorkerID);
                
                // return view to client 
                return Ok(clientToken);
            }
            Serilog.Log.Information("Remote session between user "+userAccount.FullName+" and worker "+SlaveID+" reconnected");
            return BadRequest("Device not in off remote");            
        }



        
        [HttpGet("Setting")]
        public async Task<IActionResult> GetSetting(string token)
        {

            var request = new RestRequest(_config.SessionTokenValidator)
                .AddQueryParameter("token", token);
            request.Method = Method.POST;

            var result = await _sessionToken.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var accession = JsonConvert.DeserializeObject<SessionAccession>(result.Content);
                if(accession.Module == Module.CLIENT_MODULE)
                {
                    Serilog.Log.Information("Got Session setting request from client");
                    var clientSession = await _cache.GetClientSessionSetting(accession);
                    Serilog.Log.Information(JsonConvert.SerializeObject(clientSession));
                    return Ok(clientSession);
                }
                else
                {
                    Serilog.Log.Information("Got Session setting request from worker");
                    var workerSession = await _cache.GetWorkerSessionSetting(accession);
                    Serilog.Log.Information(JsonConvert.SerializeObject(workerSession));
                    return Ok(workerSession);
                }
            }
            else
            {
                Serilog.Log.Information("Fail to parse token");
                return BadRequest("Token is invalid");
            }
        }

        [HttpPost("Setting")]
        public async Task<IActionResult> SetSetting(string token,[FromBody]UserSetting setting)
        {

            var request = new RestRequest(_config.SessionTokenValidator)
                .AddQueryParameter("token", token);
            request.Method = Method.POST;

            var result = await _sessionToken.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var accession = JsonConvert.DeserializeObject<SessionAccession>(result.Content);

                foreach (var item in _db.Clusters.ToList())
                {
                    if((await _cache.GetClusterSnapshot(item.ID)).ContainsKey(accession.WorkerID))
                    {
                        await _cache.SetSessionSetting(accession.ID,setting,_config, item);
                        return Ok();
                    }
                }
                return BadRequest();
            }
            else
            {
                Serilog.Log.Information("Fail to parse token");
                return BadRequest("Token is invalid");
            }
        }
    }
}
