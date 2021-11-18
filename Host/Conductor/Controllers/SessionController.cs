using MersenneTwister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedHost.Models;
using Conductor.Services;
using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using Conductor.Models;
using System.Linq;
using System.Threading.Tasks;
using Conductor;
using static System.Environment;
using System.Configuration;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using System.Security.Claims;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using SharedHost;
using SharedHost.Auth.ThinkmayAuthProtocol;

namespace Conductor.Controllers
{
    /// <summary>
    /// Routes related to session initialize/termination process
    /// </summary>
    [User]
    [ApiController]
    [Route("/Session")]
    [Produces("application/json")]
    public class SessionController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly SystemConfig Configuration;

        private readonly IAdmin _admin;

        private readonly RestClient Signalling;

        private readonly ISlaveManagerSocket _slmsocket;

        private readonly UserManager<UserAccount> _userManager;

        public SessionController(ApplicationDbContext db,
                                SystemConfig config, 
                                IAdmin admin,
                                ISlaveManagerSocket slmsocket,
                                UserManager<UserAccount> userManager)
        {
            _db = db;
            _admin = admin;
            _slmsocket = slmsocket;
            _userManager = userManager;
            
            Configuration = config;
            Signalling = new RestClient(Configuration.Signalling+"/System");
        }

        /// <summary>
        /// initialize session
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("Initialize")]
        public async Task<IActionResult> Create(int SlaveID)
        {
            var UserID = HttpContext.Items["UserID"];
            var Query = await _slmsocket.GetSlaveState(SlaveID);

            // search for availability of slave device
            if (Query.SlaveServiceState != SlaveServiceState.Open) { return BadRequest("Device Not Available"); }

            var sessionPair = new SessionPair()
            {
                SessionClientID = Randoms.Next(),
                SessionSlaveID = Randoms.Next()
            };

            /*create new session with gevin session request from user*/
            var sess = new RemoteSession(sessionPair,Configuration)
            {
                Client = await _userManager.FindByIdAsync(UserID.ToString()),
                Worker = _db.Devices.Find(SlaveID)
            };

            /*create session from client device capability*/
            sess.QoE = new QoE(sess.Client.DefaultSetting);

            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Generate")
                .AddJsonBody(sessionPair);

            // return bad request if fail to create 
            var reply = Signalling.Post(get_req); 
            if(reply.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest(reply.Content.ToString());
            }

            // construct client session and slave session
            SlaveSession slaveSes = new SlaveSession(sess,Configuration);
            ClientSession clientSes = new ClientSession(sess,Configuration);

            // invoke session initialization in slave pool
            await _slmsocket.SessionInitialize(slaveSes);

            // report new session to admin
            await _admin.ReportNewSession(sess);

            // return view for user
            return Ok(clientSes);
        }


    

        /// <summary>
        /// Terminate session 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpDelete("Terminate")]
        public async Task<IActionResult> Terminate(int SlaveID)
        {
            var UserID = HttpContext.Items["UserID"];
            var userAccount = await _userManager.FindByIdAsync(UserID.ToString());

            var device = _db.Devices.Find(SlaveID);

            // get session information in database
            var ses = _db.RemoteSessions.Where(s => s.Worker == device
                                               && s.Client == userAccount
                                              && !s.EndTime.HasValue).FirstOrDefault();

            // return badrequest if session is not available in database
            if (ses == null) return BadRequest();

            var sessionPair = new SessionPair()
            {
                SessionClientID = ses.SessionClientID,
                SessionSlaveID = ses.SessionSlaveID
            };

            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Terminate")
                .AddJsonBody(sessionPair);

            // return bad request if fail to delete session pair      
            var deletion_reply = Signalling.Post(get_req); 
            if(deletion_reply.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest(deletion_reply.Content.ToString());
            }

            var Query = await _slmsocket.GetSlaveState(ses.Worker.ID);

            /*slavepool send terminate session signal*/
            if(Query.SlaveServiceState == SlaveServiceState.OnSession
            || Query.SlaveServiceState == SlaveServiceState.OffRemote)
            {
                // report to admmin in case termination return successfully 
                await _admin.ReportSessionTermination(ses);

                await _slmsocket.SessionTerminate(ses.Worker.ID);
                return Ok($"Session {ses.SessionClientID} termination done");
            }
            return BadRequest("Cannot send terminate session signal to slave");            
        }


        /// <summary>
        /// disconnect remote control during session
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpPost("Disconnect")]
        public async Task<IActionResult> DisconnectRemoteControl(int SlaveID)
        {
            // get ClientId from request         
            var UserID = HttpContext.Items["UserID"];
            var userAccount = await _userManager.FindByIdAsync(UserID.ToString());

            var device = _db.Devices.Find(SlaveID);

            // get session from database
            var ses = _db.RemoteSessions.Where(s => s.Worker == device 
                                               && s.Client == userAccount 
                                              && !s.EndTime.HasValue).FirstOrDefault();

            // return bad request if session is not found in database
            if (ses == null) return BadRequest();

            var Query = await _slmsocket.GetSlaveState(ses.Worker.ID);

            /*slavepool send terminate session signal*/
            if (Query.SlaveServiceState == SlaveServiceState.OnSession)
            {
                // report remote control disconnect to admin
                await _admin.ReportRemoteControlDisconnected(ses);

                // send disconnect signal to slave
                await _slmsocket.RemoteControlDisconnect(ses.Worker.ID);
                return Ok();
            }
            return BadRequest("Device not in session");            
        }

        /// <summary>
        /// Reconnect remote control after disconnect
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpPost("Reconnect")]
        public async Task<IActionResult> ReconnectRemoteControl(int SlaveID)
        {
            // get ClientId from user request
            var UserID = HttpContext.Items["UserID"];
            var userAccount = await _userManager.FindByIdAsync(UserID.ToString());

            var device = _db.Devices.Find(SlaveID);

            // get session from database
            var ses = _db.RemoteSessions.Where(s => s.Worker == device 
                                               && s.Client == userAccount 
                                              && !s.EndTime.HasValue).FirstOrDefault();

            // return null if session is not found
            if (ses == null) return BadRequest();

            var Query = await _slmsocket.GetSlaveState(ses.Worker.ID);

            /*slavepool send terminate session signal*/
            if (Query.SlaveServiceState == SlaveServiceState.OffRemote)
            {
                // reconect remote control
                await _slmsocket.RemoteControlReconnect(ses.Worker.ID);   

                // report session reconnect to admin
                await _admin.ReportRemoteControlReconnect(ses);

                // return view to client
                ClientSession clientSes = new ClientSession(ses,Configuration);   
                return Ok(clientSes);
            }
            return BadRequest("Device not in off remote");            
        }
    }
}
