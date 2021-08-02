using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using SlaveManager.Services;
using System;
using System.Collections.Generic;

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
        }


        /// <summary>
        /// Queries for every slave device in the system for serving state and static information 
        /// </summary>
        /// <returns></returns>
        [HttpGet("System")]
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
        /// <param name="id"></param>
        /// <param name="order"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("SSH")]
        public IActionResult CommandLine(int id, int order, string command)
        {
            return (_slavePool.SendCommand(id, order, command) ? Ok() : BadRequest());
        }

        /// <summary>
        /// Add a specific Slave device 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet("AddSlave")]
        public IActionResult AddSlaveDevice(int ID)
        {
            return (_slavePool.AddSlaveId(ID) ? Ok() : BadRequest());
        }
    }
}