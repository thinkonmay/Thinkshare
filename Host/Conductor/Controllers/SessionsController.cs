using Conductor.Data;
using MersenneTwister;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using SharedHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conductor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // TODO: Add URL routing for REST requests for signalling & slave manager servers
    public class SessionsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SessionsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // POST: ManageSessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(SessionRequest req)
        {
            if (ModelState.IsValid)
            {
                int sessionSlaveId = Randoms.Next();
                int sessionClientId = Randoms.Next();

                SystemSession sysSes = new SystemSession()
                {
                    ClientID = req.ClientId,
                    SlaveID = req.SlaveId,
                    SessionSlaveID = sessionSlaveId,
                    SessionClientID = sessionClientId,
                    ClientOffer = true, // Arbitrary value
                    QoE = new QoE(),
                    SignallingUrl = GeneralConstants.SIGNALLING_SERVER,
                    StunServer = GeneralConstants.STUN_SERVER
                };

                _db.Sessions.Add(sysSes);
                await _db.SaveChangesAsync();

                var signalPair = new SessionRequest()
                {
                    SlaveId = sessionSlaveId,
                    ClientId = sessionClientId
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
                client = new RestClient(GeneralConstants.SLAVE_MANAGER_SERVER);
                client.Post(new RestRequest(JsonConvert.SerializeObject(slaveSes), DataFormat.Json));

                return Ok();
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Terminate(int sessionClientId)
        {
            SystemSession ses = _db.Sessions.Where(s => s.SessionClientID == sessionClientId).FirstOrDefault();

            if (ses == null) return BadRequest();

            var deletion = new
            { 
                SessionClientId = ses.SessionClientID,
                SessionSlaveId = ses.SessionSlaveID
            };

            var client = new RestClient(GeneralConstants.SLAVE_MANAGER_SERVER);
            client.Delete(new RestRequest(JsonConvert.SerializeObject(deletion), DataFormat.Json));

            _db.Sessions.Remove(ses);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
