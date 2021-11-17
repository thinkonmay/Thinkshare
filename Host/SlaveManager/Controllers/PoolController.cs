using Microsoft.AspNetCore.Mvc;
using SharedHost;
using SharedHost.Models.Device;
using WorkerManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkerManager.Controllers
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
        /// Reject slave from slavepool but still keep it device infromation in database. 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpDelete("Reject")]
        public IActionResult RejectSlave(int SlaveID)
        {
            return _slavePool.RejectSlave(SlaveID) ? Ok() : BadRequest();
        }
    }
}
