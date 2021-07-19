using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SlaveManager.Interfaces;
using Newtonsoft.Json;
using SlaveManager.Services;
using SlaveManager.Slave;
using System.Web;
using SharedHost.Models;

namespace SlaveManager.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("/[controller]/[action]")]
    public class ConductorController : ControllerBase
    {
        private const string CONDUCTOR_URL = "";

        /// <summary>
        /// 
        /// </summary>
        public AgentHubHandler Hub;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_agent"></param>
        public ConductorController(AgentHubHandler _agent)
        {
            Hub = _agent;
        }

        /// <summary>
        /// Initialize session on a particular slave device
        /// </summary>
        /// <param name="slaveid">slave id of invoked slave id</param>
        /// <param name="session">string contain json serialized Session class,
        /// provide sufficient information for a remote control session</param>
        /// <returns></returns>
        [HttpPost, ActionName("SessionInitialize")]
        public IActionResult SessionInitialize(int slaveid, string session)
        {
            SlaveSession SlaveSession;
            try
            {
                 SlaveSession = JsonConvert.DeserializeObject<SlaveSession>(session);
            }
            catch
            {
                return BadRequest("Invalid session json object");
            }


            SlaveDevice slave;
            if (!Hub.SlaveList.TryGetValue(slaveid, out slave))
            {
                return NotFound("SlaveID not in queue");
            }

            var respond = slave.SessionInitialize(SlaveSession);

            if (respond.Item1)
            {
                return Ok(respond.Item2);
            }
            else
            {
                return BadRequest(respond.Item2);
            }
        }

        /// <summary>
        /// Terminate session on a particular slave device, 
        /// apply for both normal session and off remote session
        /// </summary>
        /// <param name="slaveid">SlaveID of slave device to terminate session</param>
        /// <returns></returns>
        [HttpPost, ActionName("SessionTerminate")]
        public IActionResult SessionTerminate(int slaveid)
        {
            SlaveDevice slave;
            if(!Hub.SlaveList.TryGetValue(slaveid, out slave))
            {
                return NotFound("SlaveID not in queue");
            }

            var respond = slave.SessionTerminate();

            if (respond.Item1)
            {
                return Ok(respond.Item2);
            }
            else
            {
                return BadRequest(respond.Item2);
            }
        }

        /// <summary>
        /// Disconnect remote control for an on-session slave
        /// </summary>
        /// <param name="slaveid">id of slave to disconnnect remote control</param>
        /// <returns></returns>
        [HttpPost, ActionName("RemoteControlDisconnect")]
        public IActionResult RemoteControlDisconnect(int slaveid)
        {
            SlaveDevice slave;
            if (!Hub.SlaveList.TryGetValue(slaveid, out slave))
            {
                return NotFound("SlaveID not in queue");
            }

            var respond = slave.RemoteControlDisconnect();

            if (respond.Item1)
            {
                return Ok(respond.Item2);
            }
            else
            {
                return BadRequest(respond.Item2);
            }
        }


        /// <summary>
        /// reconnect remote control for an off-remote slave
        /// </summary>
        /// <param name="slaveid">slave id of slave device to disconnect</param>
        /// <returns></returns>
        [HttpPost, ActionName("RemoteControlReconnect")]
        public IActionResult RemoteControlReconnect(int slaveid)
        {
            SlaveDevice slave;
            if (!Hub.SlaveList.TryGetValue(slaveid, out slave))
            {
                return NotFound("SlaveID not in queue");
            }

            var respond = slave.RemoteControlReconnect();

            if (respond.Item1)
            {
                return Ok(respond.Item2);
            }
            else
            {
                return BadRequest(respond.Item2);
            }
        }

        /// <summary>
        /// Send command line to slave device, 
        /// apply for all state (except disconnected and offline slave)
        /// </summary>
        /// <param name="slaveid"></param>
        /// <param name="order">id of cmd child process of agent, ranged between 0 and 8</param>
        /// <param name="command">string commnad to send to slave device</param>
        /// <returns></returns>
        [HttpPost, ActionName("SendCommand")]
        public IActionResult SendCommand(int slaveid, int order, string command)
        {
            SlaveDevice slave;
            if (!Hub.SlaveList.TryGetValue(slaveid, out slave))
            {
                return NotFound("SlaveID not in queue");
            }

            var respond = slave.SendCommand(order, command);

            if (respond.Item1)
            {
                return Ok(respond.Item2);
            }
            else
            {
                return BadRequest(respond.Item2);
            }
        }


        /// <summary>
        /// RejectSlave, this action end agent process on slave device 
        /// and delete slavedevice from serving queue
        /// </summary>
        /// <param name="slaveid">id of slave device to reject</param>
        /// <returns></returns>
        [HttpPost, ActionName("RejectSlave")]
        public IActionResult RejectSlave(int slaveid)
        {
            SlaveDevice slave;
            if (!Hub.SlaveList.TryGetValue(slaveid, out slave))
            {
                return NotFound("SlaveID not in queue");
            }


            var respond = slave.RejectSlave();
            Hub.SlaveList.TryRemove(Hub.SlaveList.First(item => item.Key.Equals(slaveid)));

            if (respond.Item1)
            {
                return Ok(respond.Item2);
            }
            else
            {
                return BadRequest(respond.Item2);
            }
        }

        /// <summary>
        /// Disconnect slave from connection with host, 
        /// only end agent process on slave device without delete slave from serving queue,
        /// set slavestate to offline
        /// </summary>
        /// <param name="slaveid"></param>
        /// <returns></returns>
        [HttpPost, ActionName("DisconnectSlave")]
        public IActionResult DisconnectSlave(int slaveid)
        {
            SlaveDevice slave;
            if (!Hub.SlaveList.TryGetValue(slaveid, out slave))
            {
                return NotFound("SlaveID not in queue");
            }


            var respond = slave.RejectSlave();

            var disconnected = new DeviceDisconnected();
            slave.State = disconnected;

            if (respond.Item1)
            {
                return Ok(respond.Item2);
            }
            else
            {
                return BadRequest(respond.Item2);
            }
        }


        /// <summary>
        /// Add new slave id to the serving queue, 
        /// slave value will not be added until agent register.
        /// slave device to host wil specified slave id
        /// </summary>
        /// <param name="slaveid">SlaveID to be added</param>
        /// <returns></returns>
        [HttpPost, ActionName("AddSlaveID")]
        public IActionResult AddSlaveId(int slaveid)
        {
            DeviceDisconnected offline = new DeviceDisconnected();
            SlaveDevice slave = new SlaveDevice(offline,null);

            if(Hub.SlaveList.TryAdd(slaveid, slave))
            {
                return Ok("SlaveId Added");
            }
            else
            {
                return NotFound("SlaveID have slready in queue");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpGet,ActionName("GetSystemSlaveState")]
        public IActionResult GetSystemSlaveState()
        {
            IEnumerable<KeyValuePair<int, SlaveDevice>> slave_enum = 
                (IEnumerable<KeyValuePair<int, SlaveDevice>>)Hub.SlaveList.GetEnumerator();

            Dictionary<int, string> slave_state_enum = new Dictionary<int, string>();
            foreach (KeyValuePair<int,SlaveDevice> i in slave_enum)
            {
                slave_state_enum.Add( i.Key, i.Value.State.state );
            }
            return Ok(JsonConvert.SerializeObject(slave_state_enum));
        }
    }
}
