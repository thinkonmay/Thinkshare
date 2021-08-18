using MersenneTwister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Administration;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using SlaveManager.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using SlaveManager;
using static System.Environment;
using System.Configuration;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Net;


// TODO: authentification

namespace SlaveManager.Controllers
{
    [Route("/Session")]
    [ApiController]
    //user
    // TODO: Add URL routing for REST requests for signalling & slave manager servers
    public class SessionsController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly ISlavePool _slavePool;

        private readonly SystemConfig Configuration;

        private readonly IAdmin _admin;

        private readonly RestClient Signalling;

        public SessionsController(ApplicationDbContext db,SystemConfig config, ISlavePool slavePool, IAdmin admin)
        {
            _db = db;
            _admin = admin;
            _slavePool = slavePool;
            Configuration = config;
            
            Signalling = new RestClient("http://"+Configuration.BaseUrl+":"+ Configuration.SignallingPort+"/System");
        }

        /// <summary>
        /// initialize session
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="SlaveId"></param>
        /// <param name="ScreenWidth"></param>
        /// <param name="ScreenHeight"></param>
        /// <param name="bitrate"></param>
        /// <param name="QoEMode"></param>
        /// <param name="VideoCodec"></param>
        /// <param name="AudioCodec"></param>
        /// <returns></returns>
        [HttpGet("Initialize")]
        public async Task<IActionResult> Create(int ClientId, int SlaveId, int ScreenWidth, int ScreenHeight,int bitrate, int QoEMode, int VideoCodec, int AudioCodec)
        {
            var cap = new ClientDeviceCapabilities();
            cap.screenWidth= ScreenWidth;
            cap.screenHeight = ScreenHeight;
            cap.bitrate = bitrate;
            cap.audioCodec = (Codec)AudioCodec;
            cap.videoCodec = (Codec)VideoCodec;
            cap.mode = (QoEMode)QoEMode;

            var req = new ClientRequest();
            req.cap = cap;
            req.ClientId = ClientId;
            req.SlaveId = SlaveId;

            ///search for availability of slave device
            if (_slavePool.GetSlaveState(SlaveId) != SlaveServiceState.Open) { return BadRequest("Device Not Available"); }
            await _admin.ReportNewSession(SlaveId, ClientId);

            /*create session id pair randomly*/
            int sessionSlaveId = Randoms.Next();
            int sessionClientId = Randoms.Next();

            /*create session from client device capability*/
            var _QoE = new QoE(cap);

            /*create new session with gevin session request from user*/
            Session sess = new Session(req, _QoE, sessionSlaveId, sessionClientId,
                "ws://" + Configuration.BaseUrl +":"+ Configuration.SignallingPort + "/Session",
                Configuration.StunServer);

            _db.Sessions.Add(sess);
            await _db.SaveChangesAsync();

            var signalPair = new SessionPair()
            {
                SessionSlaveID = sessionSlaveId,
                SessionClientID = sessionClientId
            };


            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Generate")
                .AddParameter("SessionSlaveID", sessionSlaveId.ToString())
                .AddParameter("SessionClientID", sessionClientId.ToString());            
            var reply = Signalling.Get(get_req); // TODO post and get confirmation from signalling server
            if(reply.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest(reply.Content.ToString());
            }
           

            SlaveSession slaveSes = new SlaveSession(sess,Configuration.StunServerLibsoup);
            ClientSession clientSes = new ClientSession(sess,Configuration.StunServer);

            if(!_slavePool.SessionInitialize(SlaveId, slaveSes))
            {
                var error = new GeneralErrorAbsTime();
                error.ErrorMessage = "Cannot send initialize signal to agent";
                error.ErrorTime = 0;
                _admin.ReportAgentError(error,SlaveId);
                return BadRequest("Cannot send session initialize signal to slave");

            }

            SessionViewModel view = new SessionViewModel();
            view.clientSession = clientSes; 
            view.ClientID = sess.ClientID;
            view.HostUrl = "http://"+Configuration.BaseUrl+":"+ Configuration.SlaveManagerPort;
            view.DevMode = false;
            return View("RemoteControl",view);

        }

        /// <summary>
        /// Terminate session 
        /// </summary>
        /// <param name="sessionClientId"></param>
        /// <returns></returns>
        [HttpDelete("Terminate")]
        public async Task<IActionResult> Terminate(int sessionClientId)
        {
            
            Session ses = _db.Sessions.Where(s => s.SessionClientID == sessionClientId).FirstOrDefault();
            if(ses == null)
            {
                return BadRequest("session not found");
            }
            await _admin.ReportSessionTermination(ses.SlaveID, ses.ClientID);

            ses.EndTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            if (ses == null) return BadRequest();

            /*generate rest post to signalling server*/
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Terminate")
                .AddParameter("SessionSlaveID", ses.SessionSlaveID.ToString())
                .AddParameter("SessionClientID", ses.SessionClientID.ToString());            
            var deletion_reply = Signalling.Get(get_req); // TODO post and get confirmation from signalling server
            if(deletion_reply.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest(deletion_reply.Content.ToString());
            }



            /*slavepool send terminate session signal*/
            if(!_slavePool.SessionTerminate(ses.SlaveID))
            {
                return BadRequest("Cannot send terminate session signal to slave");
            }
            return Ok($"Session {ses.SessionClientID} termination done");
        }


        /// <summary>
        /// disconnect remote control during session
        /// </summary>
        /// <param name="sessionClientId"></param>
        /// <returns></returns>
        [HttpDelete("Disconnect")]
        public async Task<IActionResult> DisconnectRemoteControl(int sessionClientId)
        {
            Session ses = _db.Sessions.Where(s => s.SessionClientID == sessionClientId).FirstOrDefault();

            if (ses == null) return BadRequest();


            /*slavepool send terminate session signal*/
            if (_slavePool.RemoteControlDisconnect(ses.SlaveID))
            {
                return Ok();
            }
            else
            {
                return BadRequest("Device not in session");
            }
        }

        /// <summary>
        /// Reconnect remote control after disconnect
        /// </summary>
        /// <param name="sessionClientId"></param>
        /// <returns></returns>
        [HttpPost("Reconnect")]
        public async Task<IActionResult> ReconnectRemoteControl(int sessionClientId)
        {
            Session ses = _db.Sessions.Where(s => s.SessionClientID == sessionClientId).FirstOrDefault();

            if (ses == null) return BadRequest();

            /*slavepool send terminate session signal*/
            if (_slavePool.RemoteControlReconnect(ses.SlaveID))
            {
                ClientSession clientSes = new ClientSession(ses,Configuration.StunServer);                
                SessionViewModel view = new SessionViewModel();
                view.clientSession = clientSes; 
                view.ClientID = ses.ClientID;
                view.HostUrl = "http://"+Configuration.BaseUrl+":"+ Configuration.SlaveManagerPort;
                view.DevMode = false;
                return View("RemoteControl",view);
            }
            else
            {
                return BadRequest("Device not in off remote");
            }
        }
    }
}
