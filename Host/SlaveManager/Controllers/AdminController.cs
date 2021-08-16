using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using SlaveManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlaveManager.Controllers
{
    [Route("/Admin")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly ISlavePool _slavePool;

        private readonly ApplicationDbContext _db;

        public AdminController(ISlavePool slavePool, ApplicationDbContext db)
        {
            _slavePool = slavePool;
            _db = db;

            var list = _db.Devices.ToList();
            foreach (var i in list)
            {
                slavePool.AddSlaveId(i.ID);
            }
        }


        /// <summary>
        /// Queries for every slave device in the system for serving state and static information 
        /// </summary>
        /// <returns></returns>
        [HttpGet("System")]
        //manager
        public IActionResult System()
        {
            var system = _slavePool.GetSystemSlaveState();
            List<Tuple<Slave, string>> resp = new List<Tuple<Slave, string>>();

            foreach (var i in system)
            {
                var device = _db.Devices.Find(i.Item1);
                resp.Add(new Tuple<Slave, string>(device, i.Item2));
            }
            return Ok(JsonConvert.SerializeObject(resp));
        }


        /// <summary>
        /// Send a command line to an specific process id of an specific slave device
        /// </summary>
        /// <param name="slave_id"></param>
        /// <param name="process_id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("ForwardCommand")]
        public IActionResult CommandLine(int slave_id, int process_id, string command)
        {
            return (_slavePool.SendCommand(slave_id, process_id, command) ? Ok() : BadRequest());
        }

        /// <summary>
        /// Add a specific Slave device 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost("AddSlave")]
        public IActionResult AddSlaveDevice(int ID)
        {
            return _slavePool.AddSlaveId(ID) ? Ok() : BadRequest();
        }

        /// <summary>
        /// Reject slave from slavepool but still keep it device infromation in database. 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet("RejectSlave")]
        public IActionResult RejectSlave(int ID)
        {
            return _slavePool.RejectSlave(ID) ? Ok() : BadRequest();
        }


        /// <summary>
        /// Disconnect slave but still keep its ID in slavepool
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpDelete("DisconnectSlave")]
        public IActionResult DisconnectSlave(int ID)
        {
            return _slavePool.DisconnectSlave(ID) ? Ok() : BadRequest();        
        }

        //         /// <summary>
        // /// initialize session
        // /// </summary>
        // /// <param name="req"></param>
        // /// <returns></returns>
        // [HttpPost("Initialize")]
        // public async Task<IActionResult> Create(ClientRequest req)
        // {

        //     if (ModelState.IsValid)
        //     {
        //         if (_slavePool.GetSlaveState(req.SlaveId) != SlaveServiceState.Open) { return BadRequest("Device Not Available"); }
        //         await _admin.ReportNewSession(req.SlaveId, 0);

        //         /*create session id pair randomly*/
        //         int sessionSlaveId = Randoms.Next();
        //         int sessionClientId = Randoms.Next();

        //         /*create session from client device capability*/
        //         var _QoE = new QoE(req.cap);

        //         /*create new session with gevin session request from user*/
        //         Session sess = new Session(req, _QoE, sessionSlaveId, sessionClientId,
        //             "ws://" + Configuration.BaseUrl +":"+ Configuration.SignallingPort + "/Session",
        //             Configuration.StunServer);

        //         _db.Sessions.Add(sess);
        //         await _db.SaveChangesAsync();

        //         var signalPair = new SessionPair()
        //         {
        //             SessionSlaveID = sessionSlaveId,
        //             SessionClientID = sessionClientId
        //         };


        //         /*generate rest post to signalling server*/
        //         var signalling_post = new RestRequest(
        //             $"System/Generate?SessionSlaveID={signalPair.SessionSlaveID}&SessionClientID={signalPair.SessionClientID}");
                
        //         var reply = Signalling.Post(signalling_post);

        //         SlaveSession slaveSes = new SlaveSession(sess,Configuration.StunServerLibsoup);
        //         ClientSession clientSes = new ClientSession(sess,Configuration.StunServer);

        //         if(!_slavePool.SessionInitialize(req.SlaveId, slaveSes))
        //         {
        //             return BadRequest("Cannot send session initialize signal to slave");
        //         }

        //         SessionViewModel view = new SessionViewModel();
        //         view.clientSession = clientSes; 
        //         view.ClientID = sess.ClientID;
        //         view.HostUrl = "http://"+Configuration.BaseUrl+":"+ Configuration.SlaveManagerPort;
        //         view.DevMode = true;
        //         return View("RemoteControl",view);
        //     }

        //     return BadRequest();
        // }


        // /// <summary>
        // /// Terminate session 
        // /// </summary>
        // /// <param name="sessionClientId"></param>
        // /// <returns></returns>
        // [HttpDelete("Terminate")]
        // public async Task<IActionResult> Terminate(int sessionClientId)
        // {
            
        //     Session ses = _db.Sessions.Where(s => s.SessionClientID == sessionClientId).FirstOrDefault();

        //     ses.EndTime = DateTime.UtcNow;
        //     await _db.SaveChangesAsync();

        //     if (ses == null) return BadRequest();

        //     var deletion = new SessionPair()
        //     {
        //         SessionClientID = ses.SessionClientID,
        //         SessionSlaveID = ses.SessionSlaveID
        //     };

        //     /*create rest delete to signalling server*/

        //     /*generate rest post to signalling server*/
        //     var signalling_delete = new RestRequest($"System/Terminate?SessionSlaveID=${deletion.SessionSlaveID}&SessionClientID=${deletion.SessionClientID}");

        //     var reply = Signalling.Delete(signalling_delete);

        //     /*slavepool send terminate session signal*/
        //     if(_slavePool.SessionTerminate(ses.SlaveID))
        //     {
        //         return BadRequest("Cannot send terminate session signal to slave");
        //     }
        //     return Ok();
        // }




    }
}