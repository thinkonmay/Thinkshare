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

namespace SlaveManager.Controllers
{
    [Route("/User")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ISlavePool _SlavePool;

        private readonly ApplicationDbContext _db;
        public UserController(ISlavePool slavePool, ApplicationDbContext db)
        {
            _SlavePool = slavePool;
            _db = db;
        }

        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        [HttpGet("FetchSlave")]
        public async Task<IActionResult> UserGetCurrentAvailableDevice(int UserID)
        {
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
                        ID = slave.ID
                    };
                    resp.Add(device_infor);
                }
            }
            return Ok(JsonConvert.SerializeObject(resp));
        }
    }
}
