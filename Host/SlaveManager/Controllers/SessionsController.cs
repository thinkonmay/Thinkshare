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
using Microsoft.AspNetCore.Identity;
using SlaveManager.Models.User;
using System.Security.Claims;


// TODO: authentification

namespace SlaveManager.Controllers
{
    [Route("/Session")]
    [ApiController]
    [Authorize]
    public class SessionsController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly ISlavePool _slavePool;

        private readonly SystemConfig Configuration;

        private readonly IAdmin _admin;

        private readonly RestClient Signalling;
        
        private readonly UserManager<UserAccount> _userManager;

        private readonly ITokenGenerator _jwt;

        private readonly string SignallingUrl; 

        private readonly string SlaveManagerUrl;

        public SessionsController(ApplicationDbContext db,
                                SystemConfig config, 
                                ISlavePool slavePool, 
                                IAdmin admin,
                                ITokenGenerator jwt,
                                UserManager<UserAccount> userManager)
        {
            _db = db;
            _admin = admin;
            _slavePool = slavePool;
            _userManager = userManager;
            _jwt = jwt;
            
            Configuration = config;
            Signalling = new RestClient("http://"+Configuration.BaseUrl+":"+ Configuration.SignallingPort+"/System");
            SignallingUrl = "ws://" + Configuration.BaseUrl +":"+ Configuration.SignallingPort + "/Session";
            SlaveManagerUrl = "http://"+Configuration.BaseUrl+":"+ Configuration.SlaveManagerPort;
        }

        /// <summary>
        /// initialize session
        /// </summary>
        /// <param name="SlaveId"></param>
        /// <param name="ScreenWidth"></param>
        /// <param name="ScreenHeight"></param>
        /// <param name="QoEMode"></param>
        /// <param name="VideoCodec"></param>
        /// <param name="AudioCodec"></param>
        /// <returns></returns>
        [HttpGet("Initialize")]
        public async Task<IActionResult> Create(int SlaveId, 
                                                int ScreenWidth, 
                                                int ScreenHeight,
                                                int QoEMode, 
                                                int VideoCodec, 
                                                int AudioCodec)
        {
            int ClientId = _jwt.GetUserFromHttpRequest(User);

            // construct client device capability information
            var cap = new ClientDeviceCapabilities();
            cap.screenWidth= ScreenWidth;
            cap.screenHeight = ScreenHeight;
            cap.audioCodec = (Codec)AudioCodec;
            cap.videoCodec = (Codec)VideoCodec;
            cap.mode = (QoEMode)QoEMode;

            // construct client request
            var req = new ClientRequest();
            req.cap = cap;
            req.ClientId = ClientId;
            req.SlaveId = SlaveId;

            // search for availability of slave device
            if (_slavePool.GetSlaveState(SlaveId) != SlaveServiceState.Open) { return BadRequest("Device Not Available"); }

            /*create session id pair randomly*/
            int sessionSlaveId = Randoms.Next();
            int sessionClientId = Randoms.Next();

            /*create session from client device capability*/
            var _QoE = new QoE(cap);

            /*create new session with gevin session request from user*/
            Session sess = new Session(req, 
                                    _QoE, 
                                    sessionSlaveId, 
                                    sessionClientId,
                                    SignallingUrl,
                                    Configuration.StunServer);

            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Generate")
                .AddParameter("SessionSlaveID", sessionSlaveId.ToString())
                .AddParameter("SessionClientID", sessionClientId.ToString());  

            // return bad request if fail to create 
            var reply = Signalling.Get(get_req); 
            if(reply.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest(reply.Content.ToString());
            }
           
            // construct client session and slave session
            SlaveSession slaveSes = new SlaveSession(sess,Configuration.StunServerLibsoup);
            ClientSession clientSes = new ClientSession(sess,Configuration.StunServer);

            // invoke session initialization in slave pool
            _slavePool.SessionInitialize(SlaveId, slaveSes);

            // report new session to admin
            await _admin.ReportNewSession(SlaveId, ClientId);

            // return view for user
            SessionViewModel view = new SessionViewModel();
            view.clientSession = clientSes; 
            view.ClientID = sess.ClientID;
            view.HostUrl = SlaveManagerUrl;
            view.DevMode = _jwt.IsAdmin(User);
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
            //get client id from request
            var ClientId =  _jwt.GetUserFromHttpRequest(User);

            // get session information in database
            Session ses = _db.Sessions.Where(s => s.SessionClientID == sessionClientId 
                                               && s.ClientID == ClientId  
                                              && !s.EndTime.HasValue).FirstOrDefault();

            // return badrequest if session is not available in database
            if (ses == null) return BadRequest();

            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Terminate")
                .AddParameter("SessionSlaveID", ses.SessionSlaveID.ToString())
                .AddParameter("SessionClientID", ses.SessionClientID.ToString());      

            // return bad request if fail to delete session pair      
            var deletion_reply = Signalling.Get(get_req); 
            if(deletion_reply.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest(deletion_reply.Content.ToString());
            }



            /*slavepool send terminate session signal*/
            if(_slavePool.GetSlaveDevice(ses.SlaveID).GetSlaveState() == SlaveServiceState.OnSession
            || _slavePool.GetSlaveDevice(ses.SlaveID).GetSlaveState() == SlaveServiceState.OffRemote)
            {
                // report to admmin in case termination return successfully 
                await _admin.ReportSessionTermination(ses);

                // 
                _slavePool.SessionTerminate(ses.SlaveID);
                return Ok($"Session {ses.SessionClientID} termination done");
            }
            return BadRequest("Cannot send terminate session signal to slave");            
        }


        /// <summary>
        /// disconnect remote control during session
        /// </summary>
        /// <param name="sessionClientId"></param>
        /// <returns></returns>
        [HttpDelete("Disconnect")]
        public IActionResult DisconnectRemoteControl(int sessionClientId)
        {   
            // get ClientId from request         
            var ClientId =  _jwt.GetUserFromHttpRequest(User);

            // get session from database
            Session ses = _db.Sessions.Where(s => s.SessionClientID == sessionClientId 
                                               && s.ClientID == ClientId 
                                              && !s.EndTime.HasValue).FirstOrDefault();

            // return bad request if session is not found in database
            if (ses == null) return BadRequest();

            /*slavepool send terminate session signal*/
            if (_slavePool.GetSlaveDevice(ses.SlaveID).GetSlaveState() == SlaveServiceState.OnSession)
            {
                // report remote control disconnect to admin
                _admin.ReportRemoteControlDisconnected(ses);

                // send disconnect signal to slave
                _slavePool.RemoteControlDisconnect(ses.SlaveID);
                return Ok();
            }
            return BadRequest("Device not in session");            
        }

        /// <summary>
        /// Reconnect remote control after disconnect
        /// </summary>
        /// <param name="sessionClientId"></param>
        /// <returns></returns>
        [HttpPost("Reconnect")]
        public IActionResult ReconnectRemoteControl(int sessionClientId)
        {            
            // get ClientId from user request
            var ClientId =  _jwt.GetUserFromHttpRequest(User);

            // get session from database
            Session ses = _db.Sessions.Where(s => s.SessionClientID == sessionClientId 
                                               && s.ClientID == ClientId 
                                              && !s.EndTime.HasValue).FirstOrDefault();

            // return null if session is not found
            if (ses == null) return BadRequest();

            /*slavepool send terminate session signal*/
            if (_slavePool.GetSlaveDevice(ses.SlaveID).GetSlaveState() == SlaveServiceState.OffRemote)
            {
                // reconect remote control
                _slavePool.RemoteControlReconnect(ses.SlaveID);   

                // report session reconnect to admin
                _admin.ReportRemoteControlReconnect(ses);

                // return view to client
                ClientSession clientSes = new ClientSession(ses,Configuration.StunServer);   
                SessionViewModel view = new SessionViewModel();
                view.clientSession = clientSes; 
                view.ClientID = ses.ClientID;
                view.HostUrl = SlaveManagerUrl;
                view.DevMode = _jwt.IsAdmin(User);
                return View("RemoteControl",view);
            }
            return BadRequest("Device not in off remote");            
        }
    }
}
