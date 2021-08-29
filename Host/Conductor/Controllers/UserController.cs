using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedHost.Models;
using Conductor.Data;
using Conductor.Interfaces;
using Conductor.Models;
using Conductor.Services;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Conductor.Models.User;
using SharedHost.Models.Device;

namespace Conductor.Controllers
{
    [Route("/User")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly ApplicationDbContext _db;

        private readonly ISlaveManagerSocket _slmsocket;

        private readonly ITokenGenerator _jwt;

        public UserController(ApplicationDbContext db, 
                            UserManager<UserAccount> userManager,
                            ISlaveManagerSocket slm,
                            ITokenGenerator jwt)
        {
            _slmsocket = slm;
            _db = db;
            _userManager = userManager;
            _jwt = jwt;
        }





        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchSlave")]
        public async Task<IActionResult> UserGetCurrentAvailableDevice()
        {

            List<SlaveDeviceInformation> resp = new List<SlaveDeviceInformation>();
            var stateList = await _slmsocket.GetSystemSlaveState();

            foreach (var i in stateList)
            {
                if (String.Equals(i.SlaveServiceState, SlaveServiceState.Open))
                {
                    // Add Device Information to open device Id list;
                    var slave = _db.Devices.Find(i.SlaveID);

                    var device_infor = new SlaveDeviceInformation(slave);
                    resp.Add(device_infor);
                }
            }
            return Ok(JsonConvert.SerializeObject(resp));
        }



        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchSession")]
        public async Task<IActionResult> UserGetCurrentSesssion()
        {
            int ClientId = _jwt.GetUserFromHttpRequest(User);
            var resp = new List<SlaveDeviceInformation>();

            var session = _db.RemoteSessions.Where(s => s.ClientID == ClientId
                                         && !s.EndTime.HasValue);

            foreach (var i in session)
            {
                // Add Device Information to open device Id list;
                var slave = _db.Devices.Find(i.SlaveID);

                var Query = await _slmsocket.GetSlaveState(i.SlaveID);

                var device_infor = new SlaveDeviceInformation(slave)
                {
                    SessionClientID = i.SessionClientID,
                    serviceState = Query.SlaveServiceState
                };
                resp.Add(device_infor);                
            }
            return Ok(JsonConvert.SerializeObject(resp));
        }
    }
}
