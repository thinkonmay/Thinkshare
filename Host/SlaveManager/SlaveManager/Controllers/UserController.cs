﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using SlaveManager.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;






// Authentification mechanism

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


        [HttpGet("FetchSlave")]
        public async Task<IActionResult> UserGetCurrentAvailableDevice(int UserID)
        {
            // TODO 1: provide user authentification mechanism

            // TODO 2: provide 
            List<Slave> resp = new List<Slave>();
            var stateList = _SlavePool.GetSystemSlaveState();

            foreach (var i in stateList)
            {
                if (i.Item2 == SlaveServiceState.Open)
                {
                    // Add Device Information to open device Id list;
                    var member = _db.Devices.Where(o => o.ID == i.Item1).FirstOrDefault();
                    resp.Add(member);
                }
            }
            return Ok(JsonConvert.SerializeObject(resp));
        }
    }
}
