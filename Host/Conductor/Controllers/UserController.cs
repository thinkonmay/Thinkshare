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
using SharedHost.Models.User;
using SharedHost.Models.Device;
using SharedHost.Models.Session;

namespace Conductor.Controllers
{
    /// <summary>
    /// Routes used by user to fetch information about the system
    /// </summary>
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

                    var device_infor = new SlaveDeviceInformation(slave)
                    {
                        serviceState = SlaveServiceState.Open
                    };
                    resp.Add(device_infor);
                }
            }
            return Ok(resp);
        }



        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchSession")]
        public async Task<IActionResult> UserGetCurrentSesssion()
        {
            int ClientId = _jwt.GetUserFromHttpRequest(User);

            var account = await _userManager.FindByIdAsync(ClientId.ToString());

            var session = _db.RemoteSessions.Where(s => s.Client == account
                                                    && !s.EndTime.HasValue).ToList();
            
            var ret = new List<SlaveDeviceInformation>();

            foreach (var i in session)
            {
                var device_infor = new SlaveDeviceInformation(i.Slave);

                var Query = await _slmsocket.GetSlaveState(i.Slave.ID);
                device_infor.SessionClientID = i.SessionClientID;
                device_infor.serviceState = Query.SlaveServiceState;
                ret.Add(device_infor);                
            }

            // search for remote session with client id and endtime equal null
            return Ok(ret);
        }


        [HttpGet("GetInfor")]
        public async Task<IActionResult> UserGetInfor()
        {
            int ClientId = _jwt.GetUserFromHttpRequest(User);
            var account = await _userManager.FindByIdAsync(ClientId.ToString());

            return Ok(account);
        }


        [HttpGet("GetSession")]
        public async Task<IActionResult> UserGetSession()
        {
            int ClientId = _jwt.GetUserFromHttpRequest(User);
            var sessions = _db.RemoteSessions.Where( o => o.ClientId == ClientId);
            return Ok(sessions);
        }
    }
}
