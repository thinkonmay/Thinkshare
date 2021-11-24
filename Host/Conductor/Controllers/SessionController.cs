using Microsoft.AspNetCore.Mvc;
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

        private readonly SystemConfig _config;

        private readonly RestClient _sessionToken;

        private readonly IWorkerCommnader _Cluster;

        private readonly UserManager<UserAccount> _userManager;

        public SessionController(ApplicationDbContext db,
                                SystemConfig config,
                                IWorkerCommnader slmsocket,
                                UserManager<UserAccount> userManager)
        {
            _db = db;
            _Cluster = slmsocket;
            _userManager = userManager;
            
            _config = config;
            _sessionToken = new RestClient(_config.Authenticator+"/Token");
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
            var Query = _db.Devices.Find(SlaveID).WorkerState;

            // search for availability of slave device
            if (Query != WorkerState.Open) { return BadRequest("Device Not Available"); }

            /*create new session with gevin session request from user*/
            var sess = new RemoteSession(_config)
            {
                Client = await _userManager.FindByIdAsync(UserID.ToString()),
                Worker = _db.Devices.Find(SlaveID)
            };

            /*create session from client device capability*/
            sess.QoE = new QoE(sess.Client.DefaultSetting);


            /*generate rest post to signalling server*/
            var workerTokenRequest = new RestRequest("GrantSession")
                .AddJsonBody(new SessionAccession
                {
                    ClientID = (int)UserID,
                    WorkerID = sess.Worker.ID,
                    ID = sess.ID,
                    Module = Module.CORE_MODULE
                });

            var clientTokenRequest = new RestRequest("GrantSession")
                .AddJsonBody(new SessionAccession
                {
                    ClientID = (int)UserID,
                    WorkerID = sess.Worker.ID,
                    ID = sess.ID,
                    Module = Module.CLIENT_MODULE
                });

            // return bad request if fail to delete session pair      
            var clientToken = _sessionToken.Post(clientTokenRequest).Content;
            var workerToken = _sessionToken.Post(workerTokenRequest).Content;


            // invoke session initialization in slave pool
            await _Cluster.SessionInitialize(SlaveID, workerToken ,
                new SessionBase 
                { 
                    SignallingUrl = _config.SignallingWs,
                    QoE = sess.QoE 
                });

            // return view for user
            return Ok(clientToken);
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
            var State = device.WorkerState;

            // get session information in database
            var ses = _db.RemoteSessions.Where(s => s.Worker == device && 
                                               s.Client == userAccount && 
                                              !s.EndTime.HasValue);

            // return badrequest if session is not available in database
            if (!ses.Any()) return BadRequest();


            /*slavepool send terminate session signal*/
            if(State == WorkerState.OnSession
            || State == WorkerState.OffRemote)
            {
                //
                await _Cluster.SessionTerminate(ses.First().WorkerID);
                return Ok();
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

            var Query = (await _db.Devices.FindAsync(ses.Worker.ID)).WorkerState;

            /*slavepool send terminate session signal*/
            if (Query == WorkerState.OnSession)
            {
                // send disconnect signal to slave
                await _Cluster.SessionDisconnect(ses.Worker.ID);
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
            var ses = _db.RemoteSessions.Where(s => s.Worker == device && 
                                               s.Client == userAccount && 
                                              !s.EndTime.HasValue);
            if (!ses.Any()) { return BadRequest(); }


            var clientTokenRequest = new RestRequest("GrantSession")
                .AddJsonBody(new SessionAccession
                {
                    ClientID = (int)UserID,
                    WorkerID = device.ID,
                    ID = ses.First().ID,
                    Module = Module.CLIENT_MODULE
                });

            // return bad request if fail to delete session pair      
            var clientToken = _sessionToken.Post(clientTokenRequest);

            // return null if session is not found
            if (ses == null) return BadRequest();

            var Query = _db.Devices.Find(SlaveID).WorkerState;

            /*slavepool send terminate session signal*/
            if (Query == WorkerState.OffRemote)
            {
                // reconect remote control
                await _Cluster.SessionReconnect(ses.First().WorkerID,new SessionBase());

                // return view to client 
                return Ok(clientToken);
            }
            return BadRequest("Device not in off remote");            
        }
    }
}
