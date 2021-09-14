using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Conductor.Data;
using Conductor.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using SharedHost.Models.Error;
using System.Linq;
using SharedHost;

namespace Conductor.Controllers
{
    /// <summary>
    /// Routes used by admin to query current information of the system
    /// </summary>
    [Authorize(Roles = "Administrator")]
    [Route("/Query")]
    [ApiController]
    [Produces("application/json")]
    public class QueryController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly ApplicationDbContext _db;

        private readonly ISlaveManagerSocket _slmsocket;

        private readonly IAdmin _admin;

        public QueryController( ApplicationDbContext db, 
                                IAdmin admin, 
                                ISlaveManagerSocket slmsocket,
                                UserManager<UserAccount> userManager,
                                SystemConfig config)
        {
            _userManager = userManager;
            _slmsocket = slmsocket;
            _admin = admin;
            _db = db;
        }





        /// <summary>
        /// Queries for every slave device in the system for serving state and static information 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Device")]
        //manager
        public async Task<IActionResult> System()
        {
            var Query = await _slmsocket.GetSystemSlaveState();
            var resp = new List<SlaveDeviceInformation>();

            foreach (var i in Query)
            {
                var slavedb = _db.Devices.Find(i.SlaveID);

                var SlaveDevice = new SlaveDeviceInformation(slavedb)
                {
                    serviceState = i.SlaveServiceState
                };
                resp.Add(SlaveDevice);
            }
            return Ok(resp);
        }




        /// <summary>
        /// Queries for every slave device in the system for serving state and static information 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Session")]
        //manager
        public async Task<IActionResult> GetSession(int SlaveID)
        {

            var Query = _db.Devices.Find(SlaveID).servingSession.ToList();
            return Ok(Query);
        }

        /// <summary>
        /// Queries for every slave device in the system for serving state and static information 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Command")]
        //manager
        public async Task<IActionResult> GetCommand(int SlaveID)
        {
            var slave = _db.Devices.Find(SlaveID);
            var Query = _db.CommandLogs.Where(o => o.Slave == slave).ToList();
            return Ok(Query);
        }
    }
}