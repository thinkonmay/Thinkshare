using System.Threading.Tasks;
using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using MersenneTwister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedHost;
using SharedHost.Models.Device;
using SharedHost.Auth.ThinkmayAuthProtocol;

namespace Conductor.Controllers
{
    [Admin]
    [Route("/Device")]
    [ApiController]
    [Produces("application/json")]
    public class DeviceController : Controller
    {


        private readonly ISlaveManagerSocket _slmsocket;

        private readonly ApplicationDbContext _db;

        private readonly SystemConfig _config;

        public DeviceController(ISlaveManagerSocket slmSocket,
                                ApplicationDbContext db,
                                SystemConfig config)
        {
            _slmsocket = slmSocket;
            _db = db;
            _config = config;
        }
        /// <summary>
        /// Add a specific Slave device 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Add")]
        public async Task<IActionResult> AddSlaveDevice()
        {
            var slave = new Slave();
            slave.ID = Randoms.Next();
            _db.Devices.Add(slave);
            await _db.SaveChangesAsync();

            var host_config = new HostConfiguration()
            {
                SlaveID = slave.ID,
                HostUrl = _config.SlaveManagerWs,
                DisableSSL = true
            };
            return Ok(host_config);
        }

        /// <summary>
        /// Reject slave from slavepool but still keep it device infromation in database. 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpDelete("Reject")]
        public async Task<IActionResult> RejectSlave(int ID)
        {
            return await _slmsocket.RejectSlave(ID) ? Ok() : BadRequest();
        }


        /// <summary>
        /// Disconnect slave but still keep its ID in slavepool
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpDelete("Disconnect")]
        public async Task<IActionResult> DisconnectSlave(int ID)
        {
            return await _slmsocket.DisconnectSlave(ID) ? Ok() : BadRequest();
        }
    }
}
