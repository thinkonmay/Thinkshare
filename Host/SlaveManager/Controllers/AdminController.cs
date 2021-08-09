using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedHost.Models;
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
            return _slavePool.DisconnectSlave(ID) ? Ok() : BadRequest();        }



    }
}