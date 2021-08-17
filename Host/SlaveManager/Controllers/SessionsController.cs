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
            
            Signalling = new RestClient("http://"+Configuration.BaseUrl+":"+ Configuration.SignallingPort);
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
            var signalling_post = new RestRequest(
                $"System/Generate?SessionSlaveID={signalPair.SessionSlaveID}&SessionClientID={signalPair.SessionClientID}");
            
            var reply = Signalling.Post(signalling_post); // TODO post and get confirmation from signalling server
            // if(reply.Content != "Added session pair")
            // {
            //     return BadRequest(reply.Content);
            // }

            /*Check for session pair in session */
            // var signalling_get = new RestRequest("​/System​/GetCurrentSession");
            // var currentsession_res = Signalling.Get(signalling_get);

            // var respond = JsonConvert.DeserializeObject<List<Tuple<int, int>>>(currentsession_res.Content);
            // if(respond.Where(o => o.Item1 == sessionClientId && o.Item2 == sessionSlaveId).Count() == 0)
            // {
            //     return BadRequest("Session key pair not found after generate");
            // }             

            SlaveSession slaveSes = new SlaveSession(sess,Configuration.StunServerLibsoup);
            ClientSession clientSes = new ClientSession(sess,Configuration.StunServer);

            if(!_slavePool.SessionInitialize(SlaveId, slaveSes))
            {
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

            var deletion = new SessionPair()
            {
                SessionClientID = ses.SessionClientID,
                SessionSlaveID = ses.SessionSlaveID
            };

            /*create rest delete to signalling server*/

            /*generate rest post to signalling server*/
            var signalling_delete = new RestRequest($"System/Terminate?SessionSlaveID=${deletion.SessionSlaveID}&SessionClientID=${deletion.SessionClientID}");

            var reply = Signalling.Delete(signalling_delete); // TODO delete and get confirmation from signalling server
            // if (reply.Content != "Terminated session pair")
            // {
            //     return BadRequest("Fail to remove session key pair");
            // }

            // var signalling_get = new RestRequest("​/System​/GetCurrentSession");
            // var currentsession_res = Signalling.Get(signalling_get);

            // var respond = JsonConvert.DeserializeObject<List<Tuple<int, int>>>(currentsession_res.Content);
            // if (respond.Where(o => o.Item1 == sessionClientId && o.Item2 == deletion.SessionSlaveID).Count() == 1)
            // {
            //     return BadRequest("Fail to delete session key pair");
            // }

            /*slavepool send terminate session signal*/
            if(_slavePool.SessionTerminate(ses.SlaveID))
            {
                return BadRequest("Cannot send terminate session signal to slave");
            }
            return Ok();
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
