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
        public UserController(ISlavePool slavePool, ApplicationDbContext db, UserManager<UserAccount> userManager)
        {
            _SlavePool = slavePool;
            _db = db;
            _userManager = userManager;
        }

        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchSlave")]
        public async Task<IActionResult> UserGetCurrentAvailableDevice()
        {
            var user = _userManager.GetUserAsync(User);

            List<SlaveDeviceInformation> resp = new List<SlaveDeviceInformation>();
            var stateList = _SlavePool.GetSystemSlaveState();

            foreach (var i in stateList)
            {
                if (String.Equals(i.Item2, SlaveServiceState.Open))
                {
                    // Add Device Information to open device Id list;
                    var slave = _db.Devices.Find(i.Item1);

                    var device_infor = new SlaveDeviceInformation()
                    {
                        CPU = slave.CPU,
                        GPU = slave.GPU,
                        RAMcapacity = slave.RAMcapacity,
                        OS = slave.OS,
                        ID = slave.ID,
                    SessionClientID = null
                    };
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
            var user = _userManager.GetUserAsync(User);

            List<SlaveDeviceInformation> resp = new List<SlaveDeviceInformation>();

            var session = _db.Sessions.Where(
                s => s.ClientID == user.Id  && !s.EndTime.HasValue);

            foreach (var i in session)
            {
                // Add Device Information to open device Id list;
                var slave = _db.Devices.Find(i.SlaveID);

                var device_infor = new SlaveDeviceInformation()
                {
                    CPU = slave.CPU,
                    GPU = slave.GPU,
                    OS = slave.OS,
                    ID = slave.ID,
                    serviceState = _slavePool.GetSlaveDevice(i.SlaveID).GetSlaveState(),
                    RAMcapacity = slave.RAMcapacity,
                    SessionClientID = session.SessionClientID
                };
                resp.Add(device_infor);                
            }
            return Ok(JsonConvert.SerializeObject(resp));
        }
    }
}
