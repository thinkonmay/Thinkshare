using MersenneTwister;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using SharedHost.Models;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using SlaveManager.Services;
using System;
using System.Linq;
using System.Threading.Tasks;




// TODO: authentification

namespace SlaveManager.Controllers
{
    [Route("/Session")]
    [ApiController]
    // TODO: Add URL routing for REST requests for signalling & slave manager servers
    public class SessionsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        private readonly ISlavePool _slavePool;

        public SessionsController(ApplicationDbContext db, ISlavePool slavePool)
        {
            _db = db;

            _slavePool = slavePool;
        }

        // POST: ManageSessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("Request")]
        public async Task<IActionResult> Create(ClientRequest req)
        {
            if (ModelState.IsValid)
            {
                if (_slavePool.GetSlaveState(req.SlaveId) != "Device Open") { return BadRequest("Device Not Available"); }

                /*create session id pair randomly*/
                int sessionSlaveId = Randoms.Next();
                int sessionClientId = Randoms.Next();

                /*create session from */
                var _QoE = new QoE();
                _QoE.ScreenHeight = req.cap.screenHeight;
                _QoE.ScreenWidth = req.cap.screenWidth;
                _QoE.Bitrate = req.cap.bitrate;
                _QoE.VideoCodec = req.cap.videoCodec;
                _QoE.AudioCodec = req.cap.audioCodec;
                _QoE.QoEMode = req.cap.mode;

                Session sess = new Session()
                {
                    ClientID = req.ClientId,
                    SlaveID = req.SlaveId,
                    SessionSlaveID = sessionSlaveId,
                    SessionClientID = sessionClientId,
                    ClientOffer = false, // Arbitrary value
                    QoE = _QoE,
                    SignallingUrl = GeneralConstants.SIGNALLING_SERVER,
                    StunServer = GeneralConstants.STUN_SERVER
                };

                _db.Sessions.Add(sess);
                await _db.SaveChangesAsync();

                var signalPair = new SessionPair()
                {
                    SessionSlaveID = sessionSlaveId,
                    SessionClientID = sessionClientId
                };

                var client = new RestClient(GeneralConstants.SIGNALLING_SERVER);
                client.Post(new RestRequest(JsonConvert.SerializeObject(signalPair), DataFormat.Json));

                SlaveSession slaveSes = new SlaveSession()
                {
                    SessionSlaveID = sessionSlaveId,
                    SignallingUrl = GeneralConstants.SIGNALLING_SERVER,
                    StunServer = GeneralConstants.STUN_SERVER,
                    ClientOffer = true,
                    QoE = new QoE()
                };

                ClientSession clientSes = new ClientSession()
                {
                    SessionClientID = sessionClientId,
                    SignallingUrl = GeneralConstants.SIGNALLING_SERVER,
                    StunServer = GeneralConstants.STUN_SERVER,
                    ClientOffer = true,
                    QoE = new QoE()
                };
                _slavePool.SessionInitialize(sessionSlaveId, slaveSes);

                return Ok(JsonConvert.SerializeObject(clientSes));
            }

            return BadRequest();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionClientId"></param>
        /// <returns></returns>
        [HttpPost("Terminate")]
        public async Task<IActionResult> Terminate(int sessionClientId)
        {
            Session ses = _db.Sessions.Where(s => s.SessionClientID == sessionClientId).FirstOrDefault();
            ses.EndTime = DateTime.UtcNow;

            if (ses == null) return BadRequest();

            var deletion = new SessionPair()
            {
                SessionClientID = ses.SessionClientID,
                SessionSlaveID = ses.SessionSlaveID
            };

            var client = new RestClient(GeneralConstants.SLAVE_MANAGER_SERVER);
            client.Delete(new RestRequest(JsonConvert.SerializeObject(deletion), DataFormat.Json));

            _db.Sessions.Remove(ses);
            await _db.SaveChangesAsync();

            /*slavepool send terminate session signal*/
            _slavePool.SessionTerminate(ses.SessionSlaveID);
            return Ok();
        }

        [HttpPost("Disconnect")]
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
        /// 
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
                return Ok();
            }
            else
            {
                return BadRequest("Device not in off remote");
            }
        }
    }
}
