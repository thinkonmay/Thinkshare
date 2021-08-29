﻿using Microsoft.AspNetCore.Mvc;
using SharedHost;
using SharedHost.Models.Device;
using SlaveManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Controllers
{
    [Route("/Pool")]
    [ApiController]
    [Produces("application/json")]
    public class PoolController : Controller
    {
        private readonly ISlavePool _slavePool;

        private readonly IConductorSocket _conductor;

        private readonly SystemConfig _config;

        public PoolController(ISlavePool slavePool,
                                SystemConfig config,
                                IConductorSocket conductor)
        {
            _config = config;
            _conductor = conductor;
            _slavePool = slavePool;
        }

        /// <summary>
        /// Queries for every slave device in the system for serving state and static information 
        /// </summary>
        /// <returns></returns>
        [HttpGet("QueryAll")]
        //manager
        public IActionResult System()
        {
            var ret = new List<SlaveQueryResult>();
            var system = _slavePool.GetSystemSlaveState();

            return Ok(system);
        }

        /// <summary>
        /// Queries for every slave device in the system for serving state and static information 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Query")]
        //manager
        public IActionResult QuerySlaveDevice(int SlaveID)
        {
            var system = _slavePool.GetSlaveDevice(SlaveID);
            if (system != null)
            {
                var ret = new SlaveQueryResult()
                {
                    SlaveID = SlaveID,
                    SlaveServiceState = system.GetSlaveState()
                };
                return Ok(ret);
            }
            else
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// Add a specific Slave device 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        public IActionResult AddSlaveDevice(int SlaveID)
        {
            return _slavePool.AddSlaveId(SlaveID) ? Ok() : BadRequest();
        }



        /// <summary>
        /// Reject slave from slavepool but still keep it device infromation in database. 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpDelete("Reject")]
        public IActionResult RejectSlave(int SlaveID)
        {
            return _slavePool.RejectSlave(SlaveID) ? Ok() : BadRequest();
        }


        /// <summary>
        /// Disconnect slave but still keep its ID in slavepool
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpDelete("Disconnect")]
        public IActionResult DisconnectSlave(int SlaveID)
        {
            return _slavePool.DisconnectSlave(SlaveID) ? Ok() : BadRequest();
        }
    }
}
