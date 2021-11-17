using MersenneTwister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;
using SharedHost.Models;
using WorkerManager.Interfaces;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Collections.Generic;

// TODO: authentification

namespace WorkerManager.Controllers
{
    [Route("/Session")]
    [ApiController]
    [Produces("application/json")]
    public class SessionsController : Controller
    {

        private readonly ISlavePool _slavePool;

        public SessionsController(SystemConfig config, 
                                  ISlavePool slavePool)
        {
            _slavePool = slavePool;
        }

        /// <summary>
        /// initialize session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [HttpPost("Initialize")]
        public IActionResult Create([FromBody] SlaveSession session)
        {
            // invoke session initialization in slave pool
            return _slavePool.SessionInitialize(session.SlaveID, session)? Ok(): BadRequest();
        }


    

        /// <summary>
        /// Terminate session 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpPost("Terminate")]
        public async Task<IActionResult> Terminate(int SlaveID)
        {         
            /*slavepool send terminate session signal*/
            if(_slavePool.GetSlaveDevice(SlaveID).GetSlaveState() 
                is SlaveServiceState.OnSession or SlaveServiceState.OffRemote)
            {
                _slavePool.SessionTerminate(SlaveID);
                return Ok();
            }
            return BadRequest();            
        }


        /// <summary>
        /// disconnect remote control during session
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpPost("Disconnect")]
        public IActionResult DisconnectRemoteControl(int SlaveID)
        {   
            /*slavepool send terminate session signal*/
            if (_slavePool.GetSlaveDevice(SlaveID).GetSlaveState() == SlaveServiceState.OnSession)
            {
                _slavePool.RemoteControlDisconnect(SlaveID);
                return Ok();
            }
            return BadRequest();            
        }

        /// <summary>
        /// Reconnect remote control after disconnect
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpPost("Reconnect")]
        public IActionResult ReconnectRemoteControl(int SlaveID)
        {
            /*slavepool send terminate session signal*/
            if (_slavePool.GetSlaveDevice(SlaveID).GetSlaveState() == SlaveServiceState.OffRemote)
            {
                _slavePool.RemoteControlReconnect(SlaveID);   
                return Ok();
            }
            return BadRequest();            
        }
    }
}
