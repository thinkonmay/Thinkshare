using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using SlaveManager.Services;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SlaveManager.Models.User;

namespace SlaveManager.Controllers
{
    [Route("/User")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly ISlavePool _SlavePool;
        private readonly UserManager<UserAccount> _userManager;

        private readonly ApplicationDbContext _db;

        private readonly ITokenGenerator _jwt;
        public UserController(ISlavePool slavePool, 
                            ApplicationDbContext db, 
                            UserManager<UserAccount> userManager,
                            ITokenGenerator jwt)
        {
            _SlavePool = slavePool;
            _db = db;
            _userManager = userManager;
            _jwt = jwt;
        }





        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchSlave")]
        public IActionResult UserGetCurrentAvailableDevice()
        {

            List<SlaveDeviceInformation> resp = new List<SlaveDeviceInformation>();
            var stateList = _SlavePool.GetSystemSlaveState();

            foreach (var i in stateList)
            {
                if (String.Equals(i.Item2, SlaveServiceState.Open))
                {
                    // Add Device Information to open device Id list;
                    var slave = _db.Devices.Find(i.Item1);

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

            List<SlaveDeviceInformation> resp = new List<SlaveDeviceInformation>();

            var session = _db.Sessions.Where(s => s.ClientID == ClientId
                                         && !s.EndTime.HasValue);

            foreach (var i in session)
            {
                // Add Device Information to open device Id list;
                var slave = _db.Devices.Find(i.SlaveID);

                var device_infor = new SlaveDeviceInformation(slave)
                {
                    serviceState = _SlavePool.GetSlaveDevice(i.SlaveID).GetSlaveState(),
                    SessionClientID = i.SessionClientID
                };
                resp.Add(device_infor);                
            }
            return Ok(JsonConvert.SerializeObject(resp));
        }
    }
}
