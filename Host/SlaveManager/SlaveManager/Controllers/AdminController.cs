using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlaveManager.Data;
using SlaveManager.Models;
using SlaveManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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


        [HttpPost("SSH")]
        public IActionResult CommandLine(int id, int order, string command)
        {
            return (_slavePool.SendCommand(id, order, command) ? Ok() : BadRequest());
        }

        [HttpGet("AddSlave")]
        public IActionResult AddSlaveDevice(int ID)
        {
            return (_slavePool.AddSlaveId(ID) ? Ok() : BadRequest());
        }
    }
}