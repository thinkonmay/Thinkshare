using MersenneTwister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
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

        public SessionsController(ApplicationDbContext db,SystemConfig config, ISlavePool slavePool, IAdmin admin)
        {
            _db = db;
            _admin = admin;
            _slavePool = slavePool;
            Configuration = config;

        }

        /// <summary>
        /// initialize session
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Initialize")]
        public async Task<IActionResult> Create(ClientRequest req)
        {

            if (ModelState.IsValid)
            {
                if (_slavePool.GetSlaveState(req.SlaveId) != "DEVICE_OPEN") { return BadRequest("Device Not Available"); }
                await _admin.ReportNewSession(req.SlaveId, req.ClientId);

                /*create session id pair randomly*/
                int sessionSlaveId = Randoms.Next();
                int sessionClientId = Randoms.Next();

                /*create session from client device capability*/
                var _QoE = new QoE(req.cap);

                /*create new session with gevin session request from user*/
                Session sess = new Session(req, _QoE, sessionSlaveId, sessionClientId,
                    "ws://" + Configuration.BaseUrl + Configuration.SignallingPort,
                    Configuration.StunServer);

                var now = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); //port to string for compability with postgresql;
                sess.StartTime = now;
                _db.Sessions.Add(sess);
                await _db.SaveChangesAsync();

                var signalPair = new SessionPair()
                {
                    SessionSlaveID = sessionSlaveId,
                    SessionClientID = sessionClientId
                };

                /*generate rest post to signalling server*/
                var client = new RestClient("http://"+
                    Configuration.BaseUrl+
                    Configuration.SignallingPort+ 
                    "/System/Generate");
                client.Execute(new RestRequest(JsonConvert.SerializeObject(signalPair),Method.POST));

                SlaveSession slaveSes = new SlaveSession(sess,Configuration.StunServerLibsoup);
                ClientSession clientSes = new ClientSession(sess,Configuration.StunServer);

                _slavePool.SessionInitialize(sessionSlaveId, slaveSes);

                SessionViewModel view = new SessionViewModel();
                view.clientSession = clientSes; 
                view.ClientID = sess.ClientID;
                return View("RemoteControl",view);
            }

            return BadRequest();
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

            ses.EndTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); //port to string for compability with postgresql;
            await _db.SaveChangesAsync();

            if (ses == null) return BadRequest();

            var deletion = new SessionPair()
            {
                SessionClientID = ses.SessionClientID,
                SessionSlaveID = ses.SessionSlaveID
            };

            /*create rest delete to signalling server*/
            var client = new RestClient("http://"+
                Configuration.BaseUrl+
                Configuration.SignallingPort+
                "/System​/Terminate");
            client.Delete(new RestRequest(JsonConvert.SerializeObject(deletion), DataFormat.Json));

            /*slavepool send terminate session signal*/
            _slavePool.SessionTerminate(ses.SessionSlaveID);
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
            if (_slavePool.RemoteControlDisconnect(ses.SessionSlaveID))
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
            if (_slavePool.RemoteControlReconnect(ses.SessionSlaveID))
            {
                ClientSession clientSes = new ClientSession(ses,Configuration.StunServer);                
                SessionViewModel view = new SessionViewModel();
                view.clientSession = clientSes; 
                view.ClientID = ses.ClientID;
                return View("RemoteControl",view);
            }
            else
            {
                return BadRequest("Device not in off remote");
            }
        }
    }
}
